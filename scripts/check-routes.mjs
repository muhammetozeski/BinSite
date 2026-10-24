import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const repository = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const sourceRoot = process.argv[2] ? path.resolve(process.argv[2]) : path.join(repository, 'src');
const ignoredDirectories = new Set(['bin', 'obj', '.git', 'node_modules']);

// Preserve line breaks so diagnostics still refer to the original source file.
function withoutRazorComments(source) {
  return source.replace(/@\*[\s\S]*?\*@/g, comment => comment.replace(/[^\r\n]/g, ' '));
}

function normalizeRoute(route) {
  return (route.replace(/\/+$/g, '') || '/').toLowerCase();
}

async function razorFiles(directory) {
  const files = [];
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const location = path.join(directory, entry.name);
    if (entry.isDirectory() && !ignoredDirectories.has(entry.name.toLowerCase())) {
      files.push(...await razorFiles(location));
    } else if (entry.isFile() && entry.name.toLowerCase().endsWith('.razor')) {
      files.push(location);
    }
  }
  return files;
}

async function main() {
  const files = (await razorFiles(sourceRoot)).sort();
  const locations = new Map();
  let directives = 0;
  for (const file of files) {
    const source = withoutRazorComments(await readFile(file, 'utf8'));
    for (const match of source.matchAll(/^[\t ]*@page[\t ]+"((?:\\.|[^"\\])*)"/gm)) {
      const route = JSON.parse('"' + match[1] + '"');
      const normalized = normalizeRoute(route);
      const line = source.slice(0, match.index).split('\n').length;
      const found = locations.get(normalized) || [];
      found.push({ file, line, route });
      locations.set(normalized, found);
      directives++;
    }
  }

  const duplicates = [...locations].filter(([, found]) => found.length > 1);
  if (duplicates.length) {
    for (const [route, found] of duplicates) {
      console.error(`Duplicate normalized Razor route: ${route}`);
      for (const item of found) console.error(`  ${path.relative(repository, item.file)}:${item.line}  @page "${item.route}"`);
    }
    process.exitCode = 1;
    return;
  }
  console.log(`Razor routes passed: ${directives} directives in ${files.length} source files; no case or trailing-slash duplicates.`);
}

try {
  await main();
} catch (error) {
  console.error(`Razor route check failed: ${error.message}`);
  process.exitCode = 1;
}
