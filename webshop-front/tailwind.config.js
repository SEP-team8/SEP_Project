/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./index.html", "./src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    container: {
      center: true,
      padding: {
        DEFAULT: "1.5rem",
        md: "2rem",
        lg: "2.5rem",
      },
    },
    extend: {
      colors: {
        kayak: {
          50: "#f2f8fb",
          100: "#dff0fb",
          300: "#8fcfec",
          500: "#0077c8" /* primary brand blue */,
          700: "#005ea0",
        },
        accent: {
          50: "#fff3ea",
          100: "#ffead6",
          500: "#ff6a00" /* CTA orange */,
        },
      },
      boxShadow: {
        "card-md": "0 6px 18px rgba(17,24,39,0.06)",
        floating: "0 10px 30px rgba(2,6,23,0.08)",
      },
      borderRadius: {
        xl: "1rem",
      },
      fontFamily: {
        sans: ["Inter", "ui-sans-serif", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [require("@tailwindcss/forms"), require("@tailwindcss/typography")],
};
