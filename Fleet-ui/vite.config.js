import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
      port: 5173,
      watch:
      {
          ignored: ['**/.vs/**']
      },
      proxy: {
        '/api': {
          target: 'http://localhost:5067',
          changeOrigin: true,
        },
      },
  },
})
