# Creative implementation contracts, batch 002

These products own their HTML, CSS and JavaScript. No parent shell constrains their interaction. All operate in the browser; no shared database or simulated social activity is shown.

| Product | Distinct mechanism and boundary |
| --- | --- |
| Audio Workshop | Decodes user audio into the same AudioContext sampling rate, slices actual samples, optionally reverses and fades them, concatenates channels and writes interleaved signed 16-bit PCM in a RIFF/WAVE container. Unlike Rhythm Desk, this edits arbitrary audio rather than scheduling synthesized beats. Input is capped at 30 MB per file and assembly at ten minutes. |
| Audio Postcard | A seeded note sequence plus a personal text is carried in a URL fragment. The recipient hears the same procedure-generated twelve-note melody after pressing play. This is a personal message format rather than a general sequencer. |
| Light Drawing | Additive glowing strokes accumulate like a long exposure; full stroke trajectories are kept for replay, undo and local persistence. It is continuous spatial painting rather than Pixel Loop's editable animation frames. |
| Kaleidoscope | Each source stroke is transformed by radial rotations and optional mirrored transforms. Changing the symmetry reconstructs every stroke. Validated project files preserve editable source geometry; PNG is the flattened output. |
| Chain Physics | Drawn segments are static collision surfaces. Gravity-driven balls collide with surfaces and each other; hitting a target creates a new ball once. Fixed 1/120-second physics steps are separate from display frames. A URL stores the blueprint, not a fabricated replay. |
| Type Poster | Typography, font metrics and letter spacing create a vector poster in stacked, curved or rotated arrangements. It exports the same standalone SVG and a raster image, rather than using comic characters or painting. |
| Tile Workshop | A geometric tiling is built from square, split-triangle or hexagonal polygons. Hit-testing identifies individual stones for painting and motif rotation. The editable tiling is stored locally. |
| Walking Avatar | A parameterized skeleton computes limb endpoints from bone lengths and joint angles. Two saved poses interpolate continuously. This is skeletal motion, unlike the pixel animator's independent image frames. |
| Weaving Loom | A binary lift matrix chooses which warp/weft thread is visible at each crossing. Plain, twill, satin and basket algorithms create editable fabric drafts. CSV exports lift decisions, not decorative tile colors. |
| Rhythm Circles | Three independent equal subdivisions of a shared period make polyrhythms. Audio-clock scheduling and radial phase visualization differ from Rhythm Desk's sixteen-step drum pattern editor. |
| Sound Viewer | Real AnalyserNode frequency/time buffers drive spectrum, waveform, peak-bin frequency, root-mean-square amplitude and spectral centroid displays. User audio and four oscillator waves can be inspected; this is an analyzer rather than an audio editor. |
| Logic Board | Directed connections evaluate seven gate types recursively with cycle detection. A half-adder example has real sum/carry outputs; up to four inputs can be enumerated into a truth table. |
| Perspective Room | Parametric furniture boxes and room surfaces are projected through camera yaw/elevation with depth-dependent scale and face ordering. Objects have editable location, color and rotation. |
| Paper Fold | Physical millimeter dimensions generate an A4 box net, glue tabs and fold/cut guides. Oversize nets explicitly report their reduced output dimensions; printing includes a twenty-millimeter scale check. |
| Story Nodes | A directed authoring graph stores node text and two labeled transitions. Reachability is computed from the start node. Reader choices traverse the actual graph, with a visit bound for authored cycles. Unlike Hikâye Ağacı, it is an offline, entirely authored story rather than a shared contribution service. |
| Word Composition | A finite single-use word bank constrains composition to twelve words across three nonempty lines. Actual selected tokens and line breaks are checked; the check does not pretend to judge literary quality. |
| Wind Art | Pointer-drawn local vector fields accelerate particles with distance falloff, gravity and exponential drag. A separate overlay keeps editing arrows out of exported art. Shared fields reproduce parameters, not random trajectories. |
| Gesture Brush | A recorded gesture is normalized into a brush shape, then stamped along a separate painting path. Every stamp retains its original brush reference. This differs from kaleidoscope transforms and direct glowing-path painting. |
| Sound Landscape | Three filtered noise layers with low-frequency modulation and a periodic three-tone chime create mixable ambience. User-selected timers fade and stop the actual sources. No external nature recording or health outcome is claimed. |
| Cipher Post | A Latin-alphabet shift cipher or dot/dash encoding produces a playable text puzzle. The recipient receives ciphertext, clue and a non-security checksum for answer comparison. Plaintext is omitted from the share payload, but this is not secure messaging. |

## Execution evidence

All twenty scripts parsed and initialized in a mocked DOM under Node.js. Fourteen additional logic checks passed: reproducible postcard melody, finite physics with an actual target hit, skeletal bone length, plain-weave lift count, all four half-adder input combinations, circuit-cycle detection, perspective depth scaling, physical A4 output/fold guides, the twelve-word/three-line composition example, finite particle state, normalized gesture with retained brush usage, reversible alphabet shifts, SOS dot/dash encoding and Turkish answer normalization.

The WAV writer was separately executed against three signed stereo samples: container size, channel count, sample rate and left/right interleaving match their expected binary values. This is a writer-contract check, not a browser codec or listening test. Real browser audio permissions, rendering and downloads remain separate from this evidence. The catalog contains exactly twenty distinct completed entries with IDs 16–35.
