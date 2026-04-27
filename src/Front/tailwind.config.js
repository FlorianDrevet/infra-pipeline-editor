/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        'ifs-dark-blue': '#0d2f66',
        'ifs-blue': '#1565c0',
        'ifs-blue-deep': '#1a3a8a',
        'ifs-cyan': '#0288d1',
        'ifs-cyan-light': '#00bcd4',
        'ifs-cyan-deep': '#00acc1',
        'ifs-teal': '#009fbd',
        'ifs-ink': {
          900: '#0d2b4f', 800: '#103454', 700: '#1a1a2e',
          500: '#60758e', 400: '#8da4ba', 300: '#b6c3d4',
          200: '#d1d5db', 100: '#e5e7eb',
        },
        'ifs-surface': {
          0: '#ffffff', 50: '#f9fafb', 100: '#f4f9ff',
          200: '#eef5ff', 300: '#e8f4fd', tinted: '#f1f7ff',
        },
        'ifs-success': '#2e7d32',
        'ifs-error': '#c62828',
        'ifs-warning': '#b45309',
      },
      borderRadius: {
        'ifs-sm': '0.375rem', 'ifs-md': '0.75rem', 'ifs-lg': '1rem',
        'ifs-xl': '1.3rem', 'ifs-2xl': '1.4rem', 'ifs-3xl': '1.6rem',
      },
      boxShadow: {
        'ifs-xs': '0 2px 8px rgba(13, 47, 102, 0.08)',
        'ifs-sm': '0 4px 12px rgba(13, 47, 102, 0.1)',
        'ifs-md': '0 4px 16px rgba(21, 101, 192, 0.08)',
        'ifs-lg': '0 10px 28px rgba(16, 52, 86, 0.06)',
        'ifs-xl': '0 16px 40px rgba(16, 52, 86, 0.06)',
        'ifs-cta': '0 8px 24px rgba(13, 47, 102, 0.22)',
        'ifs-cta-hover': '0 12px 32px rgba(13, 47, 102, 0.3)',
      },
      backgroundImage: {
        'ifs-brand': 'linear-gradient(145deg, #0d2f66 0%, #1565c0 40%, #009fbd 100%)',
        'ifs-cta': 'linear-gradient(145deg, #0d2f66 0%, #1565c0 100%)',
        'ifs-cyan': 'linear-gradient(135deg, #0288d1, #00bcd4)',
      },
    },
  },
  plugins: [],
};

