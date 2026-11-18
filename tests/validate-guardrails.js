'use strict';

const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');

const BINARY_EXTENSIONS = new Set([
  '.png', '.ico', '.jpg', '.jpeg', '.gif', '.webp',
  '.zip', '.exe', '.dll', '.pdb', '.nupkg',
  '.woff', '.woff2', '.ttf',
]);

const ALLOWED_NUGET_PACKAGES = new Set([
  'Hardcodet.NotifyIcon.Wpf',
  'Microsoft.Web.WebView2',
]);

const ALLOWED_NUGET_FEED = 'https://api.nuget.org/v3/index.json';

const ALLOWED_HTTP_URLS = new Set([
  'http://schemas.microsoft.com/winfx/2006/xaml/presentation',
  'http://schemas.microsoft.com/winfx/2006/xaml',
]);

const ALLOWED_HTTPS_HOSTS = new Set([
  'www.google.com',
  'accounts.google.com',
  'github.com',
  'api.github.com',
]);

const FORBIDDEN_EXACT_FILES_CI = new Set([
  'convert_icon.ps1',
  'finance.html',
]);

const SECRET_RULES = [
  { label: 'GitHub classic PAT', regex: /\bghp_[A-Za-z0-9]{20,}\b/g },
  { label: 'GitHub OAuth token', regex: /\bgho_[A-Za-z0-9]{20,}\b/g },
  { label: 'GitHub fine-grained PAT', regex: /\bgithub_pat_[A-Za-z0-9_]{20,}\b/g },
  { label: 'Secret API key (sk-)', regex: /\bsk-(?:proj-|live-|test-)?[A-Za-z0-9_-]{20,}\b/g },
  { label: 'Google API key', regex: /\bAIza[0-9A-Za-z_-]{30,}\b/g },
  { label: 'AWS access key ID', regex: /\bAKIA[0-9A-Z]{16}\b/g },
  { label: 'Slack token', regex: /\bxox[baprs]-[0-9A-Za-z-]{10,}\b/g },
  { label: 'Stripe live key', regex: /\bsk_live_[0-9a-zA-Z]{16,}\b/g },
  { label: 'JWT token', regex: /\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b/g },
  { label: 'PEM private key header', regex: /-----BEGIN (?:[A-Z0-9 ]+ )?PRIVATE KEY-----/g },
  { label: 'Credentialed DB URI', regex: /\b(?:postgres(?:ql)?|mysql|mongodb(?:\+srv)?|redis|amqp):\/\/[^:\s/]+:[^@\s]+@/gi },
];

const LOCAL_PATH_RULES = [
  { label: 'Windows local user/repo path', regex: /(?:^|[^A-Za-z0-9_])([A-Za-z]:\\(?:Users|repos|backup|Source\\Repos)\\[^\s"'`]+)/gi },
  { label: 'Forward-slash Windows local path', regex: /(?:^|[^A-Za-z0-9_])([A-Za-z]:\/(?:Users|repos|backup|Source\/Repos)\/[^\s"'`]+)/gi },
  { label: 'Unix home directory path', regex: /(?:^|[^A-Za-z0-9_])(\/(?:Users|home)\/[A-Za-z0-9._-]+\/[^\s"'`]*)/g },
];

function normalizeRel(relPath) {
  return relPath.replace(/\\/g, '/').replace(/^\.\//, '').replace(/\/+$/, '');
}

function walkFallback(rootDir, currentDir, results) {
  const skipDirs = new Set(['.git', 'bin', 'obj', 'publish', 'node_modules']);
  for (const entry of fs.readdirSync(currentDir, { withFileTypes: true })) {
    const full = path.join(currentDir, entry.name);
    const rel = normalizeRel(path.relative(rootDir, full));
    if (entry.isDirectory()) {
      if (!skipDirs.has(entry.name)) {
        walkFallback(rootDir, full, results);
      }
    } else if (entry.isFile() && rel !== 'convert_icon.ps1') {
      results.push(rel);
    }
  }
}

function listCandidateFiles(rootDir) {
  try {
    const stdout = execFileSync(
      'git',
      ['ls-files', '--cached', '--others', '--exclude-standard'],
      { cwd: rootDir, encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] }
    );
    const seen = new Set();
    for (const rawLine of stdout.split(/\r?\n/)) {
      const rel = normalizeRel(rawLine.trim());
      if (!rel) continue;
      if (fs.existsSync(path.join(rootDir, rel))) {
        seen.add(rel);
      }
    }
    return Array.from(seen).sort();
  } catch {
    const fallback = [];
    walkFallback(rootDir, rootDir, fallback);
    return fallback.sort();
  }
}

function createRepoFS(rootDir, overrides = {}) {
  const virtualFiles = new Map();
  for (const [key, val] of Object.entries(overrides.files || {})) {
    virtualFiles.set(normalizeRel(key), val);
  }
  const deletedSet = new Set((overrides.deleted || []).map(normalizeRel));

  const fileSet = new Set(listCandidateFiles(rootDir));
  for (const delPath of deletedSet) {
    fileSet.delete(delPath);
  }
  for (const virtPath of virtualFiles.keys()) {
    if (!deletedSet.has(virtPath)) {
      fileSet.add(virtPath);
    }
  }

  const allFiles = Array.from(fileSet).sort();

  return {
    allFiles,
    exists(relPath) {
      const norm = normalizeRel(relPath);
      if (deletedSet.has(norm)) return false;
      if (virtualFiles.has(norm)) return true;
      return fs.existsSync(path.join(rootDir, norm));
    },
    isNonEmpty(relPath) {
      const norm = normalizeRel(relPath);
      if (deletedSet.has(norm)) return false;
      if (virtualFiles.has(norm)) return Buffer.byteLength(virtualFiles.get(norm), 'utf8') > 0;
      const abs = path.join(rootDir, norm);
      if (!fs.existsSync(abs)) return false;
      const stat = fs.statSync(abs);
      if (stat.isDirectory()) {
        return allFiles.some((f) => f.startsWith(`${norm}/`));
      }
      return stat.size > 0;
    },
    readText(relPath) {
      const norm = normalizeRel(relPath);
      if (deletedSet.has(norm)) return null;
      if (virtualFiles.has(norm)) return virtualFiles.get(norm);
      const abs = path.join(rootDir, norm);
      if (!fs.existsSync(abs) || fs.statSync(abs).isDirectory()) return null;
      return fs.readFileSync(abs, 'utf8');
    },
  };
}

function isBinaryFile(relPath) {
  return BINARY_EXTENSIONS.has(path.extname(relPath).toLowerCase());
}

function checkForbiddenFilesAndEnv(rfs, errors) {
  for (const rel of rfs.allFiles) {
    const base = path.posix.basename(rel);
    const lowerBase = base.toLowerCase();

    if (/^\.env(\..+)?$/i.test(base) && base !== '.env.example') {
      errors.push(`[FORBIDDEN_FILE] Tracked environment file is forbidden: ${rel}`);
      continue;
    }

    if (
      FORBIDDEN_EXACT_FILES_CI.has(rel.toLowerCase()) ||
      rel === 'developer_workflow.md' ||
      /\.(pem|key|pfx|p12|snk|sql|sqlite|sqlite3|db|dump|bak)$/i.test(base) ||
      /^(id_rsa|id_ed25519)(\..*)?$/i.test(base) ||
      /credentials.*\.json$/i.test(lowerBase) ||
      /service[-_]?account.*\.json$/i.test(lowerBase)
    ) {
      errors.push(`[FORBIDDEN_FILE] Forbidden key, dump, or local-helper file detected: ${rel}`);
    }
  }

  if (rfs.exists('.env.example')) {
    const envExample = rfs.readText('.env.example') || '';
    const sensitiveKeyRe = /(KEY|SECRET|TOKEN|PASSWORD|PASS|CREDENTIAL|AUTH|URI|URL|DSN|PRIVATE)/i;
    const safePlaceholderRe = /^(?:your[_-].*|replace[_-]?me|changeme|example[_-]?.*|<[^>]+>|\$\{[^}]+\}|""|'')$/i;
    for (const line of envExample.split(/\r?\n/)) {
      const trimmed = line.trim();
      if (!trimmed || trimmed.startsWith('#')) continue;
      const eqIdx = trimmed.indexOf('=');
      if (eqIdx === -1) continue;
      const key = trimmed.slice(0, eqIdx).trim();
      const val = trimmed.slice(eqIdx + 1).trim();
      if (sensitiveKeyRe.test(key) && val !== '' && !safePlaceholderRe.test(val)) {
        errors.push(`[ENV_PLACEHOLDER] .env.example key "${key}" must be empty or use a safe placeholder`);
      }
    }
  }
}

function checkSecretsAndLocalPaths(rfs, errors) {
  for (const rel of rfs.allFiles) {
    if (isBinaryFile(rel)) continue;
    const content = rfs.readText(rel);
    if (content === null) continue;

    for (const rule of SECRET_RULES) {
      rule.regex.lastIndex = 0;
      if (rule.regex.test(content)) {
        errors.push(`[SECRET_PATTERN] Potential ${rule.label} detected in ${rel}`);
      }
    }

    for (const rule of LOCAL_PATH_RULES) {
      rule.regex.lastIndex = 0;
      const match = rule.regex.exec(content);
      if (match) {
        errors.push(`[LOCAL_PATH] Hardcoded ${rule.label} ("${match[1]}") detected in ${rel}`);
      }
    }
  }
}

function checkNugetAndDependencies(rfs, errors) {
  const nugetConfig = rfs.readText('nuget.config');
  if (nugetConfig === null) {
    errors.push('[NUGET_SOURCE] Missing nuget.config');
  } else {
    const sourcesMatch = nugetConfig.match(/<packageSources>([\s\S]*?)<\/packageSources>/i);
    if (!sourcesMatch) {
      errors.push('[NUGET_SOURCE] nuget.config must define <packageSources>');
    } else {
      const block = sourcesMatch[1];
      if (!/<clear\s*\/>/i.test(block)) {
        errors.push('[NUGET_SOURCE] nuget.config <packageSources> must include <clear />');
      }
      const addTags = [...block.matchAll(/<add\b[^>]*>/gi)];
      if (addTags.length === 0) {
        errors.push('[NUGET_SOURCE] nuget.config must configure the official NuGet v3 feed');
      }
      for (const tag of addTags) {
        const valMatch = tag[0].match(/value="([^"]+)"/i);
        if (!valMatch || valMatch[1] !== ALLOWED_NUGET_FEED) {
          errors.push(`[NUGET_SOURCE] Unapproved NuGet feed in nuget.config: ${valMatch ? valMatch[1] : '(missing value)'}`);
        }
      }
    }
  }

  const csproj = rfs.readText('FinanceWidget.csproj');
  if (csproj === null) {
    errors.push('[DEPENDENCY_ALLOWLIST] Missing FinanceWidget.csproj');
  } else {
    const pkgRefs = [...csproj.matchAll(/<PackageReference\b[^>]*Include="([^"]+)"/gi)];
    for (const m of pkgRefs) {
      const pkg = m[1].trim();
      if (!ALLOWED_NUGET_PACKAGES.has(pkg)) {
        errors.push(`[DEPENDENCY_ALLOWLIST] Unapproved NuGet PackageReference "${pkg}" in FinanceWidget.csproj`);
      }
    }
  }

  if (rfs.exists('package.json')) {
    try {
      const pkgJson = JSON.parse(rfs.readText('package.json') || '{}');
      const deps = Object.keys(pkgJson.dependencies || {});
      const devDeps = Object.keys(pkgJson.devDependencies || {});
      if (deps.length > 0 || devDeps.length > 0) {
        errors.push(`[DEPENDENCY_ALLOWLIST] package.json must remain dependency-free (found: ${[...deps, ...devDeps].join(', ')})`);
      }
    } catch {
      errors.push('[DEPENDENCY_ALLOWLIST] Invalid package.json');
    }
  }
}

function checkApiHostsAndUpdater(rfs, errors) {
  const codeFiles = rfs.allFiles.filter((f) => /\.(cs|xaml)$/i.test(f));
  const urlRe = /https?:\/\/[^\s"'<>`)]+/gi;

  for (const rel of codeFiles) {
    const content = rfs.readText(rel);
    if (content === null) continue;
    for (const match of content.matchAll(urlRe)) {
      const rawUrl = match[0].replace(/[.,;]+$/, '');
      if (rawUrl.startsWith('http://')) {
        if (!ALLOWED_HTTP_URLS.has(rawUrl)) {
          errors.push(`[API_HOST_ALLOWLIST] Unapproved plaintext http:// URL "${rawUrl}" in ${rel}`);
        }
        continue;
      }

      let parsed;
      try {
        parsed = new URL(rawUrl);
      } catch {
        errors.push(`[API_HOST_ALLOWLIST] Malformed URL "${rawUrl}" in ${rel}`);
        continue;
      }

      if (!ALLOWED_HTTPS_HOSTS.has(parsed.hostname.toLowerCase())) {
        errors.push(`[API_HOST_ALLOWLIST] Unapproved external API host "${parsed.hostname}" in ${rel}`);
      }
    }
  }

  const appCs = rfs.readText('App.xaml.cs') || '';
  if (
    !appCs.includes('https://github.com/Genghis1227/FinanceWidget') ||
    !appCs.includes('https://api.github.com/repos/Genghis1227/FinanceWidget/releases/latest')
  ) {
    errors.push('[SUPPLY_CHAIN_UPDATER] App.xaml.cs must point to canonical Genghis1227/FinanceWidget GitHub & releases API URLs');
  }

  const mainCs = rfs.readText('MainWindow.xaml.cs') || '';
  if (!mainCs.includes('https://github.com/Genghis1227/FinanceWidget/blob/main/ReleaseNotes/latest_release_notes.md')) {
    errors.push('[SUPPLY_CHAIN_UPDATER] MainWindow.xaml.cs must point to canonical Genghis1227/FinanceWidget release notes URL');
  }
}

function checkVersionSync(rfs, errors) {
  const csproj = rfs.readText('FinanceWidget.csproj') || '';
  const readme = rfs.readText('README.md') || '';
  const latestNotes = rfs.readText('ReleaseNotes/latest_release_notes.md') || '';
  const appCs = rfs.readText('App.xaml.cs') || '';

  const csprojVer = csproj.match(/<Version>(\d+\.\d+\.\d+)<\/Version>/)?.[1] || null;
  const readmeVer = readme.match(/Finance Widget \(v(\d+\.\d+\.\d+)\)/)?.[1] || null;
  const notesVer = latestNotes.match(/^#\s+Finance Widget v(\d+\.\d+\.\d+) - Release Notes/m)?.[1] || null;

  if (!csprojVer || !readmeVer || !notesVer) {
    errors.push(`[VERSION_SYNC] Missing SemVer version in one or more manifests (csproj=${csprojVer}, README=${readmeVer}, ReleaseNotes=${notesVer})`);
  } else if (csprojVer !== readmeVer || csprojVer !== notesVer) {
    errors.push(`[VERSION_SYNC] Version mismatch detected (csproj=${csprojVer}, README=${readmeVer}, ReleaseNotes=${notesVer})`);
  }

  if (!appCs.includes('Assembly.GetExecutingAssembly().GetName().Version')) {
    errors.push('[VERSION_SYNC] App.xaml.cs must derive Version dynamically from Assembly.GetExecutingAssembly().GetName().Version');
  }
}

function checkAssetIntegrity(rfs, errors) {
  const csproj = rfs.readText('FinanceWidget.csproj') || '';
  if (csproj.includes('convert_icon.ps1')) {
    errors.push('[ASSET_INTEGRITY] FinanceWidget.csproj must not reference local helper script convert_icon.ps1');
  }

  const assetRefs = new Set();

  const appIconMatch = csproj.match(/<ApplicationIcon>([^<]+)<\/ApplicationIcon>/i);
  if (appIconMatch) assetRefs.add(normalizeRel(appIconMatch[1].trim()));

  for (const m of csproj.matchAll(/<(?:Resource|Content)\b[^>]*\b(?:Include|Update)="([^"]+)"/gi)) {
    assetRefs.add(normalizeRel(m[1].trim()));
  }

  for (const rel of rfs.allFiles.filter((f) => /\.(cs|xaml)$/i.test(f))) {
    const content = rfs.readText(rel) || '';
    for (const m of content.matchAll(/pack:\/\/application:,,,\/([^\s"'<>]+)/gi)) {
      assetRefs.add(normalizeRel(m[1].trim()));
    }
  }

  const readme = rfs.readText('README.md') || '';
  for (const m of readme.matchAll(/<img[^>]+src="([^"]+)"/gi)) {
    const target = m[1].trim();
    if (!/^(?:https?:|mailto:|#)/i.test(target)) {
      assetRefs.add(normalizeRel(target));
    }
  }
  for (const m of readme.matchAll(/\[[^\]]+\]\(([^)#\s]+)\)/g)) {
    const target = m[1].trim();
    if (!/^(?:https?:|mailto:|#)/i.test(target)) {
      assetRefs.add(normalizeRel(target));
    }
  }

  for (const asset of assetRefs) {
    if (!rfs.isNonEmpty(asset)) {
      errors.push(`[ASSET_INTEGRITY] Referenced asset or documentation target is missing or empty: ${asset}`);
    }
  }
}

function checkBrandingAndLicense(rfs, errors) {
  const license = rfs.readText('LICENSE');
  if (license === null) {
    errors.push('[LICENSE_GUARD] Missing MIT LICENSE file');
  } else if (
    !license.includes('MIT License') ||
    !license.includes('Copyright (c) 2026 Tony Trinh (Trinhism)') ||
    !license.includes('Permission is hereby granted, free of charge')
  ) {
    errors.push('[LICENSE_GUARD] LICENSE must preserve MIT License notice and Copyright (c) 2026 Tony Trinh (Trinhism)');
  }

  const readme = rfs.readText('README.md') || '';
  if (!readme.includes('https://trinhism.com/app/')) {
    errors.push('[BRANDING_ATTRIBUTION] README.md must preserve Trinhism attribution URL (https://trinhism.com/app/)');
  }
  if (!readme.includes('https://github.com/Genghis1227/FinanceWidget')) {
    errors.push('[BRANDING_ATTRIBUTION] README.md must preserve canonical repository URL (https://github.com/Genghis1227/FinanceWidget)');
  }
  if (!readme.includes('(LICENSE)')) {
    errors.push('[BRANDING_ATTRIBUTION] README.md must link to the repository LICENSE file');
  }
}

function checkCiAndGovernance(rfs, errors) {
  const codeowners = rfs.readText('.github/CODEOWNERS');
  if (!codeowners || !/^\*\s+@Genghis1227\b/m.test(codeowners)) {
    errors.push('[CI_GOVERNANCE] .github/CODEOWNERS must define "* @Genghis1227"');
  }

  const prTemplate = rfs.readText('.github/pull_request_template.md');
  if (!prTemplate || !prTemplate.includes('@Genghis1227') || !prTemplate.includes('validate-guardrails.js')) {
    errors.push('[CI_GOVERNANCE] .github/pull_request_template.md must require guardrail checks and @Genghis1227 review/merge');
  }

  const guardrailsWf = rfs.readText('.github/workflows/guardrails.yml');
  if (
    !guardrailsWf ||
    !/permissions:\s*\r?\n\s+contents:\s*read\b/.test(guardrailsWf) ||
    !guardrailsWf.includes('name: Repository Guardrails')
  ) {
    errors.push('[CI_GOVERNANCE] .github/workflows/guardrails.yml must define "Repository Guardrails" with contents: read');
  }

  const releaseWf = rfs.readText('.github/workflows/build-and-release.yml');
  if (!releaseWf) {
    errors.push('[CI_GOVERNANCE] Missing .github/workflows/build-and-release.yml');
  } else {
    const beforeJobs = releaseWf.split(/\r?\njobs:/)[0] || '';
    if (!/permissions:\s*\r?\n\s+contents:\s*read\b/.test(beforeJobs) || /contents:\s*write\b/.test(beforeJobs)) {
      errors.push('[CI_GOVERNANCE] build-and-release.yml must restrict top-level permissions to contents: read');
    }
    if (!releaseWf.includes("startsWith(github.ref, 'refs/tags/v')")) {
      errors.push('[CI_GOVERNANCE] build-and-release.yml must gate GitHub Release publishing strictly on v* tags');
    }
  }
}

function validateGuardrails(options = {}) {
  const rootDir = path.resolve(options.rootDir || path.join(__dirname, '..'));
  const rfs = createRepoFS(rootDir, options.overrides);
  const errors = [];

  checkForbiddenFilesAndEnv(rfs, errors);
  checkSecretsAndLocalPaths(rfs, errors);
  checkNugetAndDependencies(rfs, errors);
  checkApiHostsAndUpdater(rfs, errors);
  checkVersionSync(rfs, errors);
  checkAssetIntegrity(rfs, errors);
  checkBrandingAndLicense(rfs, errors);
  checkCiAndGovernance(rfs, errors);

  return { ok: errors.length === 0, errors };
}

function runSelfTests(rootDir) {
  const baseline = validateGuardrails({ rootDir });
  if (!baseline.ok) {
    console.error('Self-test aborted: live repository currently fails guardrails:');
    for (const err of baseline.errors) console.error(`  - ${err}`);
    return 1;
  }

  const readLive = (rel) => fs.readFileSync(path.join(rootDir, rel), 'utf8');
  const fakePat = 'ghp_' + 'a1b2c3d4e5'.repeat(4);
  const fakeWinPath = 'C:' + '\\Users\\alice\\secret-notes.txt';

  const negativeCases = [
    {
      name: 'Blocks tracked .env.production file',
      expectedTag: '[FORBIDDEN_FILE]',
      overrides: { files: { '.env.production': 'PORT=8080\n' } },
    },
    {
      name: 'Blocks non-empty credential in .env.example',
      expectedTag: '[ENV_PLACEHOLDER]',
      overrides: { files: { '.env.example': 'API_SECRET=super_live_credential_123\n' } },
    },
    {
      name: 'Blocks tracked private signing key (.pfx)',
      expectedTag: '[FORBIDDEN_FILE]',
      overrides: { files: { 'certs/signing.pfx': 'mock-binary-cert' } },
    },
    {
      name: 'Blocks committed GitHub PAT token',
      expectedTag: '[SECRET_PATTERN]',
      overrides: { files: { 'README.md': `${readLive('README.md')}\n${fakePat}\n` } },
    },
    {
      name: 'Blocks hardcoded local developer machine path',
      expectedTag: '[LOCAL_PATH]',
      overrides: { files: { 'README.md': `${readLive('README.md')}\nLocal file: ${fakeWinPath}\n` } },
    },
    {
      name: 'Blocks unapproved NuGet package source',
      expectedTag: '[NUGET_SOURCE]',
      overrides: { files: { 'nuget.config': readLive('nuget.config').replace(ALLOWED_NUGET_FEED, 'https://evil-nuget.example.com/v3/index.json') } },
    },
    {
      name: 'Blocks unapproved paid/third-party NuGet package',
      expectedTag: '[DEPENDENCY_ALLOWLIST]',
      overrides: { files: { 'FinanceWidget.csproj': readLive('FinanceWidget.csproj').replace('</ItemGroup>', '  <PackageReference Include="OpenAI" Version="2.0.0" />\n  </ItemGroup>') } },
    },
    {
      name: 'Blocks paid API or analytics host in C# code',
      expectedTag: '[API_HOST_ALLOWLIST]',
      overrides: { files: { 'App.xaml.cs': `${readLive('App.xaml.cs')}\n// https://api.polygon.io/v2/aggs\n` } },
    },
    {
      name: 'Blocks hijacked updater URL in App.xaml.cs',
      expectedTag: '[SUPPLY_CHAIN_UPDATER]',
      overrides: { files: { 'App.xaml.cs': readLive('App.xaml.cs').replace('Genghis1227/FinanceWidget', 'attacker/FinanceWidget') } },
    },
    {
      name: 'Blocks version desynchronization across manifests',
      expectedTag: '[VERSION_SYNC]',
      overrides: { files: { 'FinanceWidget.csproj': readLive('FinanceWidget.csproj').replace('<Version>1.0.0</Version>', '<Version>1.0.1</Version>') } },
    },
    {
      name: 'Blocks missing referenced asset file',
      expectedTag: '[ASSET_INTEGRITY]',
      overrides: { deleted: ['assets/screenshot_v1_0.png'] },
    },
    {
      name: 'Blocks removal of Trinhism attribution link',
      expectedTag: '[BRANDING_ATTRIBUTION]',
      overrides: { files: { 'README.md': readLive('README.md').replace('https://trinhism.com/app/', 'https://example.com/') } },
    },
    {
      name: 'Blocks deletion of MIT LICENSE file',
      expectedTag: '[LICENSE_GUARD]',
      overrides: { deleted: ['LICENSE'] },
    },
    {
      name: 'Blocks top-level contents: write in release workflow',
      expectedTag: '[CI_GOVERNANCE]',
      overrides: { files: { '.github/workflows/build-and-release.yml': readLive('.github/workflows/build-and-release.yml').replace('permissions:\n  contents: read', 'permissions:\n  contents: write').replace('permissions:\r\n  contents: read', 'permissions:\r\n  contents: write') } },
    },
  ];

  let failures = 0;
  for (const tc of negativeCases) {
    const res = validateGuardrails({ rootDir, overrides: tc.overrides });
    const matched = !res.ok && res.errors.some((e) => e.includes(tc.expectedTag));
    if (matched) {
      console.log(`PASS [self-test]: ${tc.name} -> ${tc.expectedTag}`);
    } else {
      failures += 1;
      console.error(`FAIL [self-test]: ${tc.name} (expected ${tc.expectedTag}, got: ${JSON.stringify(res.errors)})`);
    }
  }

  console.log(`Self-test summary: ${negativeCases.length - failures}/${negativeCases.length} negative tests passed.`);
  return failures === 0 ? 0 : 1;
}

if (require.main === module) {
  const rootDir = path.resolve(__dirname, '..');
  const args = new Set(process.argv.slice(2));
  if (args.has('--self-test')) {
    process.exit(runSelfTests(rootDir));
  }
  const result = validateGuardrails({ rootDir });
  if (!result.ok) {
    console.error(`Guardrails FAILED (${result.errors.length} violation(s)):`);
    for (const err of result.errors) {
      console.error(`  - ${err}`);
    }
    process.exit(1);
  }
  console.log('Repository guardrails PASSED (0 violations).');
}

module.exports = { validateGuardrails };
