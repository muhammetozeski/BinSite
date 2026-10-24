# Creative implementation contracts, batch 002

These products own their HTML, CSS and JavaScript. No parent shell constrains their interaction. All operate in the browser; no shared database or simulated social activity is shown.

| Product | Distinct mechanism and boundary |
| --- | --- |
| Ses Atölyesi | Decodes user audio into the same AudioContext sampling rate, slices actual samples, optionally reverses and fades them, concatenates channels and writes interleaved signed 16-bit PCM in a RIFF/WAVE container. Unlike Ritim Masası, this edits arbitrary audio rather than scheduling synthesized beats. Input is capped at 30 MB per file and assembly at ten minutes. |
| Sesli Kartpostal | A seeded note sequence plus a personal text is carried in a URL fragment. The recipient hears the same procedure-generated twelve-note melody after pressing play. This is a personal message format rather than a general sequencer. |
| Işık Resmi | Additive glowing strokes accumulate like a long exposure; full stroke trajectories are kept for replay, undo and local persistence. It is continuous spatial painting rather than Çizgi Döngüsü's editable animation frames. |
| Kaleydoskop | Each source stroke is transformed by radial rotations and optional mirrored transforms. Changing the symmetry reconstructs every stroke. Validated project files preserve editable source geometry; PNG is the flattened output. |
| Zincir Fizik | Drawn segments are static collision surfaces. Gravity-driven balls collide with surfaces and each other; hitting a target creates a new ball once. Fixed 1/120-second physics steps are separate from display frames. A URL stores the blueprint, not a fabricated replay. |
| Harf Afiş | Typography, font metrics and letter spacing create a vector poster in stacked, curved or rotated arrangements. It exports the same standalone SVG and a raster image, rather than using comic characters or painting. |
| Döşeme Ustası | A geometric tiling is built from square, split-triangle or hexagonal polygons. Hit-testing identifies individual stones for painting and motif rotation. The editable tiling is stored locally. |
| Yürüyen Avatar | A parameterized skeleton computes limb endpoints from bone lengths and joint angles. Two saved poses interpolate continuously. This is skeletal motion, unlike the pixel animator's independent image frames. |
| Dokuma Tezgâhı | A binary lift matrix chooses which warp/weft thread is visible at each crossing. Plain, twill, satin and basket algorithms create editable fabric drafts. CSV exports lift decisions, not decorative tile colors. |
| Ritim Çemberi | Three independent equal subdivisions of a shared period make polyrhythms. Audio-clock scheduling and radial phase visualization differ from Ritim Masası's sixteen-step drum pattern editor. |

## Execution evidence

The ten scripts parse in Node.js. The WAV writer was executed against three signed stereo samples: container size, channel count, sample rate and left/right interleaving match their expected binary values. This is a writer-contract check, not a browser codec or listening test. Real browser audio permissions, rendering and downloads remain separate from this evidence.
