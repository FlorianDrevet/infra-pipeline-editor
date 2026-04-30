import { Pipe, PipeTransform } from '@angular/core';

const KEYWORDS = [
  'param', 'var', 'resource', 'module', 'output', 'targetScope',
  'metadata', 'type', 'import', 'using', 'extension', 'existing',
  'if', 'else', 'for', 'in',
];

const TYPES = ['string', 'int', 'bool', 'object', 'array'];

const CONSTANTS = ['true', 'false', 'null'];

const HTML_ESCAPE_MAP: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
};

const TOKEN_CLASS_BY_GROUP_INDEX = [
  'bh-comment',
  'bh-comment',
  'bh-string',
  'bh-decorator',
  'bh-keyword',
  'bh-type',
  'bh-constant',
  'bh-number',
  'bh-property',
];

// Order matters: earlier groups take priority
// Groups: 1=block comment, 2=line comment, 3=string, 4=decorator, 5=keyword, 6=type, 7=constant, 8=number, 9=property accessor
const TOKEN_REGEX = new RegExp(
  [
    String.raw`(\/\*[\s\S]*?\*\/)`,                         // 1: block comment
    String.raw`(\/\/[^\n]*)`,                                   // 2: line comment
    String.raw`('(?:[^'\\]|\\.)*')`,                           // 3: single-quoted string
    String.raw`(@[a-zA-Z_][a-zA-Z0-9_]*)`,                         // 4: decorator
    String.raw`\b(${KEYWORDS.join('|')})\b`,                     // 5: keyword
    String.raw`\b(${TYPES.join('|')})\b`,                        // 6: type
    String.raw`\b(${CONSTANTS.join('|')})\b`,                    // 7: constant
    String.raw`\b(\d+(?:\.\d+)?)\b`,                          // 8: number
    String.raw`(\.[a-zA-Z_][a-zA-Z0-9_]*)`,                       // 9: property accessor
  ].join('|'),
  'g'
);

function escapeHtml(text: string): string {
  let escaped = '';

  for (const character of text) {
    escaped += HTML_ESCAPE_MAP[character] ?? character;
  }

  return escaped;
}

function highlightBicep(code: string): string {
  const escaped = escapeHtml(code);
  let highlighted = '';
  let lastIndex = 0;

  for (const match of escaped.matchAll(TOKEN_REGEX)) {
    const token = match[0];
    const startIndex = match.index ?? lastIndex;
    const tokenClass = resolveTokenClass(match);

    highlighted += escaped.slice(lastIndex, startIndex);
    highlighted += tokenClass ? `<span class="${tokenClass}">${token}</span>` : token;
    lastIndex = startIndex + token.length;
  }

  return highlighted + escaped.slice(lastIndex);
}

function resolveTokenClass(match: RegExpMatchArray): string | null {
  for (let groupIndex = 1; groupIndex <= TOKEN_CLASS_BY_GROUP_INDEX.length; groupIndex++) {
    if (match[groupIndex]) {
      return TOKEN_CLASS_BY_GROUP_INDEX[groupIndex - 1];
    }
  }

  return null;
}

@Pipe({ name: 'bicepHighlight', standalone: true })
export class BicepHighlightPipe implements PipeTransform {
  transform(code: string | null): string {
    if (!code) return '';
    return highlightBicep(code);
  }
}
