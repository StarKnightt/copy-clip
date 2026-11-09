import { copyFileSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { createContext, runInContext } from 'node:vm';

const assets = process.argv[2];
if (!assets) throw new Error('Pass the extracted @highlightjs/cdn-assets package directory.');
const source = readFileSync(join(assets, 'highlight.min.js'), 'utf8');
const context = createContext({});
runInContext(source, context);
const version = context.hljs.versionString;
const bundle = [`/*! Highlight.js ${version} browser build with bundled grammars. See highlight-LICENSE.txt. */`, source];
const languages = join(assets, 'languages');
for (const file of readdirSync(languages).filter(name => name.endsWith('.min.js')).sort()) {
  const id = file.slice(0, -'.min.js'.length);
  if (context.hljs.listLanguages().includes(id)) continue;
  const grammar = readFileSync(join(languages, file), 'utf8');
  runInContext(grammar, context, { filename: file });
  bundle.push(grammar);
}
writeFileSync(new URL('highlight.min.js', import.meta.url), bundle.join('\n;\n') + '\n');
copyFileSync(join(assets, 'LICENSE'), new URL('highlight-LICENSE.txt', import.meta.url));
console.log(`Bundled Highlight.js ${version} with ${context.hljs.listLanguages().length} registered grammars.`);
