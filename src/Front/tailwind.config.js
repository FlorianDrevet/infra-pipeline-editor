/** @type {import('tailwindcss').Config} */
// UI Refresh 2026-05 — Tailwind mirror of DS V2 tokens (dark-only).
// Source of truth: src/Front/src/scss/_tokens.scss + docs/design/ui-refresh-2026-05.md
// All colors are exposed as Tailwind utilities backed by CSS custom properties
// so theme overrides remain centralized in _tokens.scss.
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        // Brand (single desaturated blue family)
        'ifs-brand': {
          50:  'var(--ifs-brand-50)',
          100: 'var(--ifs-brand-100)',
          200: 'var(--ifs-brand-200)',
          400: 'var(--ifs-brand-400)',
          500: 'var(--ifs-brand-500)',
          600: 'var(--ifs-brand-600)',
          700: 'var(--ifs-brand-700)',
        },
        // Single accent (cyan)
        'ifs-accent': {
          500: 'var(--ifs-accent-500)',
          600: 'var(--ifs-accent-600)',
        },
        // Surfaces
        'ifs-bg':        'var(--ifs-bg)',
        'ifs-surface-1': 'var(--ifs-surface-1)',
        'ifs-surface-2': 'var(--ifs-surface-2)',
        'ifs-surface-3': 'var(--ifs-surface-3)',
        // Borders
        'ifs-border-subtle': 'var(--ifs-border-subtle)',
        'ifs-border-strong': 'var(--ifs-border-strong)',
        // Text
        'ifs-text': {
          primary:   'var(--ifs-text-primary)',
          secondary: 'var(--ifs-text-secondary)',
          muted:     'var(--ifs-text-muted)',
          disabled:  'var(--ifs-text-disabled)',
          'on-brand': 'var(--ifs-text-on-brand)',
        },
        // Semantic
        'ifs-success': 'var(--ifs-success)',
        'ifs-warning': 'var(--ifs-warning)',
        'ifs-danger':  'var(--ifs-danger)',
        'ifs-info':    'var(--ifs-info)',
      },
      borderRadius: {
        'ifs-sm':   'var(--ifs-radius-sm)',
        'ifs-md':   'var(--ifs-radius-md)',
        'ifs-lg':   'var(--ifs-radius-lg)',
        'ifs-pill': 'var(--ifs-radius-pill)',
      },
      boxShadow: {
        'ifs-sm': 'var(--ifs-shadow-sm)',
        'ifs-md': 'var(--ifs-shadow-md)',
        'ifs-lg': 'var(--ifs-shadow-lg)',
      },
      spacing: {
        'ifs-2':  'var(--ifs-space-2)',
        'ifs-4':  'var(--ifs-space-4)',
        'ifs-6':  'var(--ifs-space-6)',
        'ifs-8':  'var(--ifs-space-8)',
        'ifs-12': 'var(--ifs-space-12)',
        'ifs-16': 'var(--ifs-space-16)',
        'ifs-20': 'var(--ifs-space-20)',
        'ifs-24': 'var(--ifs-space-24)',
        'ifs-32': 'var(--ifs-space-32)',
        'ifs-40': 'var(--ifs-space-40)',
        'ifs-48': 'var(--ifs-space-48)',
        'ifs-64': 'var(--ifs-space-64)',
      },
      fontFamily: {
        sans: [
          'Inter Variable', 'Inter', 'ui-sans-serif', 'system-ui',
          '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif',
        ],
        mono: ['JetBrains Mono', 'Fira Code', 'Menlo', 'Consolas', 'monospace'],
      },
      transitionDuration: {
        'ifs-fast': '120ms',
        'ifs-base': '180ms',
        'ifs-slow': '240ms',
      },
      transitionTimingFunction: {
        'ifs-standard':   'cubic-bezier(0.2, 0, 0, 1)',
        'ifs-emphasized': 'cubic-bezier(0.3, 0, 0, 1)',
      },
    },
  },
  plugins: [],
};
