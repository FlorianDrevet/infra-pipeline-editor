import { Pipe, PipeTransform } from '@angular/core';

const KEYWORDS = [
  'param', 'var', 'resource', 'module', 'output', 'targetScope',
  'metadata', 'type', 'import', 'using', 'extension', 'existing',
  'if', 'else', 'for', 'in',
];

const TYPES = ['string', 'int', 'bool', 'object', 'array'];

const CONSTANTS = ['true', 'false', 'null'];

// Order matters: earlier groups take priority
// Groups: 1=block comment, 2=line comment, 3=string, 4=decorator, 5=keyword, 6=type, 7=constant, 8=number, 9=property accessor
const TOKEN_REGEX = new RegExp(
  [
    `(\\/\\*[\\s\\S]*?\\*\\/)`,                                   // 1: block comment
    `(\\/\\/[^\\n]*)`,                                             // 2: line comment
    `('(?:[^'\\\\]|\\\\.)*')`,                                     // 3: single-quoted string
    `(@[a-zA-Z_][a-zA-Z0-9_]*)`,                                   // 4: decorator
    `\\b(${KEYWORDS.join('|')})\\b`,                                // 5: keyword
    `\\b(${TYPES.join('|')})\\b`,                                   // 6: type
    `\\b(${CONSTANTS.join('|')})\\b`,                               // 7: constant
    `\\b(\\d+(?:\\.\\d+)?)\\b`,                                    // 8: number
    `(\\.[a-zA-Z_][a-zA-Z0-9_]*)`,                                 // 9: property accessor
  ].join('|'),
  'g'
);

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function highlightBicep(code: string): string {
  const escaped = escapeHtml(code);
  return escaped.replace(TOKEN_REGEX, (match, g1, g2, g3, g4, g5, g6, g7, g8, g9) => {
    if (g1) return `<span class="bh-comment">${match}</span>`;
    if (g2) return `<span class="bh-comment">${match}</span>`;
    if (g3) return `<span class="bh-string">${match}</span>`;
    if (g4) return `<span class="bh-decorator">${match}</span>`;
    if (g5) return `<span class="bh-keyword">${match}</span>`;
    if (g6) return `<span class="bh-type">${match}</span>`;
    if (g7) return `<span class="bh-constant">${match}</span>`;
    if (g8) return `<span class="bh-number">${match}</span>`;
    if (g9) return `<span class="bh-property">${match}</span>`;
    return match;
  });
}

@Pipe({ name: 'bicepHighlight', standalone: true })
export class BicepHighlightPipe implements PipeTransform {
  transform(code: string | null): string {
    if (!code) return '';
    return highlightBicep(code);
  }
}
