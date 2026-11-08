# Copy Clip

A copy-to-clipboard button that turns your code into a sheet of paper, folds it, and clips it to a wooden clipboard.

Click **Copy** and the snippet lifts off the card, folds in half, flies over and snaps under a steel clip, with sound to match.

## Preview

<video src="https://raw.githubusercontent.com/StarKnightt/copy-clip/main/assets/demo.mp4" controls muted loop playsinline width="540"></video>

**Live demo:** [starknightt.github.io/copy-clip](https://starknightt.github.io/copy-clip/)

Want the hands-free version? Open [`index.html#demo`](https://starknightt.github.io/copy-clip/#demo). A scripted cursor copies twice, the sheets stack, and the clip clears them before the loop restarts. Click once anywhere to turn the sound on.

## Tech Stack

- **HTML, CSS and vanilla JavaScript.** One file, no build step, no framework.
- **Inline SVG and CSS:** the clipboard, the steel clip and the paper texture are all drawn in code.
- **Web Audio API:** every sound is synthesized live. There are no audio files.
- **Clipboard API:** `navigator.clipboard.writeText`, with a hidden-textarea fallback.
- **Google Fonts:** Bricolage Grotesque for the UI and JetBrains Mono for the code.

## Installation

### One-file install

Copy [`index.html`](index.html) into your project. That's it. It has no dependencies and only reaches the network for Google Fonts.

### Run locally

```bash
git clone https://github.com/StarKnightt/copy-clip.git
cd copy-clip
npx serve .
```

Then open `http://localhost:3000`. Any static server works.

> **Why a server?** Opening the file directly works too, but `navigator.clipboard` needs a secure context. On `file://` the component falls back to the older `execCommand('copy')` path.

## Usage

The component fills its own page and scales to fit, so the easiest way to drop it in is an iframe:

```html
<iframe
  src="/copy-clip/index.html"
  title="Copy to clipboard demo"
  style="width: 100%; aspect-ratio: 4 / 5; border: 0"
  allow="clipboard-write"
></iframe>
```

To show your own snippet, edit two constants in the `<script>` block:

```js
const CODE = `your code here`;   // the plain text that goes to the clipboard
const HL = [                      // the same code, one highlighted line per entry
  '<span class="k">const</span> x = <span class="n">1</span>;',
];
```

`CODE` is what gets copied. `HL` is what's drawn on the card and the paper. Keep them in sync.

## Options

These are plain constants and CSS variables in `index.html`. There's no config object, so you edit them in place.

| Name | Default | Description |
|------|---------|-------------|
| `#demo` (URL hash) | off | Plays the scripted cursor loop instead of the interactive button. |
| `CODE` | `debounce` snippet | Plain text written to the clipboard. |
| `HL` | `debounce` snippet | Syntax-highlighted lines for the card. Classes: `k` keyword, `f` function, `n` number, `c` comment. |
| `T` | see code | Animation timeline in seconds, relative to the click (`paper`, `lift`, `fold`, `fly`, `slide`, `clipOpen`, `clipClose`, ...). |
| `MASTER` | `3` | Output gain for all sounds. |
| `SLOTS` | 3 slots | Offset and rotation of each stacked sheet. |
| `G.scale` | `.68` | Size of a sheet once it lands on the clipboard. |
| `L` | `8.6` | Length of the `#demo` loop, in seconds. |
| `--ink` | `#23211D` | Button and text color. |
| `--ok` | `#2F6B4F` | Green fill of the **Copied** state. |
| `--card` / `--paper` | `#FBFAF7` / `#FDFBF6` | Card and paper colors. |
| `--kw` `--fn` `--num` `--com` | plum, teal, olive, grey | Syntax colors. |

The clipboard holds at most **3** sheets. A 4th copy slides the oldest one out from under the clip.

## How It Works

1. **Copy.** The real text goes to the system clipboard. The button scales down, fills green and swaps to **Copied** with a check pop.
2. **Paper.** A sheet fades in under the code, so the text sits on paper. A seeded rustle plays.
3. **Lift.** The sheet tilts up slightly and its shadow grows softer and further away.
4. **Fold.** The right half rotates 180° behind the left, so the start of each line stays face-up. A crease and a shade fade in.
5. **Fly.** The folded sheet follows a curved path to the clipboard, shrinking as it goes. A whoosh pans left to right.
6. **Clip.** The steel jaw opens, the sheet slides up under it, and the jaw snaps shut with a small two-step rebound and a metallic click.
7. **Reset.** About 1.5 s later the button eases back to **Copy**.

Every visual and every sound is a pure function of time since the click. One `requestAnimationFrame` loop drives all of it, only `transform` and `opacity` change, and the loop stops when nothing is moving.

## Customization

### Colors

All colors live in `:root` at the top of the `<style>` block:

```css
:root {
  --ink: #1B2430;   /* button + text */
  --ok:  #1F6FEB;   /* "Copied" fill */
  --kw:  #B3261E;   /* keywords */
  --fn:  #0B6E4F;   /* function names */
}
```

The wood and steel colors are in the `.board` gradient and the `#mb` / `#jg` SVG gradients.

### Timing and sound

Slow the whole thing down by stretching the timeline in `T`:

```js
const T = {
  paper: [.02, .20], lift: [.04, .34], fold: [.30, .62],
  fly: [.60, 1.60],      // was [.60, 1.18]: a longer, floatier flight
  slide: [1.60, 1.86],   // shift everything after fly by the same amount
  clipOpen: 1.40, clipClose: 1.88,
  // ...
};
```

Sounds and visuals read the same `T`, so they stay in sync. To make it quieter, lower `MASTER`:

```js
const MASTER = 1.5;   // half as loud
```

Users can also mute it with the speaker button in the top-right corner.

## Accessibility

- **Reduced motion:** with `prefers-reduced-motion: reduce`, copying is instant. You get the **Copied** state only, with no sheet, no flight and no sound.
- **Keyboard:** the button is a native `<button>`, so **Enter** and **Space** work. It has a visible `:focus-visible` ring.
- **Screen readers:** an `aria-live` region announces "Copied to clipboard". The mute toggle uses `aria-pressed` and updates its label.
- **Decorative parts** (clipboard, clip, sheets, cursor) are `aria-hidden`.

## Browser Support

Works in all modern browsers:

- Chrome / Edge 90+
- Firefox 90+
- Safari 15+
- Mobile Safari / Chrome on iOS and Android

Sound starts after the first click or keypress, because browsers block audio until the user interacts with the page.

---

Built with [Claude Code](https://claude.com/claude-code).

## License

MIT. See [LICENSE](LICENSE).
