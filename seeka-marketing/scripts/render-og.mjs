// Renders public/og.png, the 1200x630 image shown when a page is shared.
// Run after changing the headline: node scripts/render-og.mjs
import { fileURLToPath } from 'node:url'
import sharp from 'sharp'

const ink = '#1b2657'
const lime = '#d4f05a'
const font = "'Helvetica Neue', Helvetica, Arial, sans-serif"

const svg = `
<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="630" viewBox="0 0 1200 630">
  <defs>
    <radialGradient id="glow" cx="0.5" cy="0.5" r="0.5">
      <stop offset="0" stop-color="${lime}" stop-opacity="0.35"/>
      <stop offset="1" stop-color="${lime}" stop-opacity="0"/>
    </radialGradient>
  </defs>
  <rect width="1200" height="630" fill="${ink}"/>
  <circle cx="1060" cy="90" r="420" fill="url(#glow)"/>
  <rect x="80" y="76" width="64" height="64" rx="14" fill="${lime}"/>
  <text x="112" y="124" text-anchor="middle" font-family="${font}" font-size="38" font-weight="900" fill="${ink}">S</text>
  <text x="164" y="121" font-family="${font}" font-size="36" font-weight="600" fill="#ffffff">Seeka</text>
  <text x="80" y="300" font-family="${font}" font-size="92" font-weight="700" fill="#ffffff" letter-spacing="-2">You applied.</text>
  <text x="80" y="404" font-family="${font}" font-size="92" font-weight="700" fill="${lime}" letter-spacing="-2">Nobody replied.</text>
  <text x="80" y="486" font-family="${font}" font-size="32" fill="#ffffff" fill-opacity="0.85">Stop applying cold. Reach a real person inside the company.</text>
  <text x="80" y="560" font-family="${font}" font-size="26" font-weight="600" fill="#ffffff" fill-opacity="0.6">seekajobs.tech</text>
</svg>`

await sharp(Buffer.from(svg)).png().toFile(fileURLToPath(new URL('../public/og.png', import.meta.url)))
console.log('Wrote public/og.png')
