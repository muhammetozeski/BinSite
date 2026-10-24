# Creative studio contracts, batch 003

The products in this batch perform more complete authoring workflows rather than adding visual themes to previous tools. Each owns its source and can be opened independently.

| Product | Implemented mechanism |
| --- | --- |
| Video Workshop | Multiple local video sources are trimmed into timeline clips with speed and captions. Canvas rendering and the user's media-element audio graph are combined into a MediaStream. A supported WebM recorder captures actual playback. The recorder pauses while each next source seeks, and final data is collected before download. Unlike the audio workshop, it edits moving picture content. |
| Shader Nodes | A directed math graph generates GLSL expressions after recursive dependency/cycle checks. Actual WebGL vertex/fragment compilation and program linking are checked before replacing the previous program. PNG, GLSL and editable graph outputs serve different authoring uses. |
| Motion Type | Text tracks store independent transform/opacity keyframes. Linear, smooth and overshooting interpolation drive a timeline. Exported HTML contains the model and its standalone playback engine; it does not depend on the editor. |
| Music Map | A listener traverses spatial note nodes, recording actual visit times. Node x-position sets stereo placement. Offline sample synthesis writes an actual signed stereo PCM WAV from the recorded route, unlike either drum sequencing or radial subdivision. |
| Shadow Stage | A point-light projection expands puppet coordinates from their depth onto the screen plane. Two actor poses interpolate; light size controls a soft-edge approximation. The layout view is separate from the projected shadow output. |
| World Weaver | Seeded interpolated noise at several scales produces elevation/moisture; the user then brushes terrain and names settlements. Terrain-weighted A* finds land-only roads. Water barriers may leave islands disconnected. Editable world data is separate from flattened PNG output. |
| Micro Book | Canvas text metrics wrap a manuscript into bounded lines and pages. Cover and page content can be edited; standalone HTML reads and prints without the editor. This produces a multi-page document rather than a single poster. |
| Exhibition Builder | Authored text/color/motif exhibits occupy a navigable 2D room. A self-contained visitor engine handles movement and nearby descriptions. HTML export embeds the data and engine; it does not pretend the portfolio directory is the exhibition. |
| Stage Cues | Cue records schedule tones, light transitions, captions and blackouts on a virtual stage. Automatic time and manual GO are distinct playback modes. A real CSV exports the authored cue sheet, with no claim of physical lighting control. |
| Fold Geometry | Four connected panels are folded by cumulative hinge transforms into ten 3D vertices and eight triangles. Projection is a view of the actual mesh; OBJ exports those same vertices/faces. Unlike Paper Fold, it computes folded geometry rather than a flat box net. |
| Explanation Studio | Diagram elements and directed arrows are combined with authored explanatory steps. Each step selects its highlighted elements. A standalone HTML export embeds the diagram model and reader engine. |
| Sound Effects Lab | Seeded noise, resonant partials, damping and sweeps are combined into editable effect layers. The engine produces actual samples for preview and WAV export rather than reporting an imagined generated sound. |
| Mini Game Engine | A tile editor and fixed-step platform engine share a level model. Movement, grounded jumping, collisions, collectible requirements, hazards and completion are real local mechanics. Standalone HTML embeds the same engine. |
| Collage Workshop | Local images are resized and retained in the project. Canvas clipping, layer transforms, opacity, grayscale and blend modes produce the actual flattened output. |
| Draw with Commands | A bounded tokenizer/parser interprets movement, pen, stack and repeat commands without eval. Segments retain their validated color and width and can be exported as SVG. |

## API implementation decisions

The video export follows the browser contracts described in [canvas captureStream](https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement/captureStream), [MediaRecorder type support](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder/isTypeSupported_static), and [the final dataavailable event](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder/dataavailable_event). These are documentation-backed contracts, not proof of a successful recording on every browser. The implementation chooses an available WebM format, records in real time, and downloads only after stop has delivered the final data. Hidden-tab cancellation prevents describing background-throttled export as complete.

WebGL uses the actual [compileShader API](https://developer.mozilla.org/en-US/docs/Web/API/WebGLRenderingContext/compileShader). It checks compile/link status and keeps the active vertex buffer alive with the program. A cyclic graph leaves the previous successful image in place and reports the graph error.

## Execution evidence

The first ten studio scripts parse in Node.js. The spatial composition WAV writer was executed with stereo samples; its channel count, byte length and interleaved signed sample values matched the expected container. Folded-mesh execution preserved every 200-unit hinge and 100-unit panel edge. The terrain pathfinder found a short land path and returned no path across a complete water barrier. This does not establish native video codec support, actual GPU compilation, media permissions or perceptual audio quality.

## English product contract

All forty authored creative entry points across batches 001–003 were converted to English, including starter stories, puzzles, word banks, templates, error messages, accessible labels and text embedded in independent exports. The existing three catalogs contain thirty-five English records under `Creative tools`; the last five written studio sources were not added to the catalog before the instruction to stop new production.

Interface styles now use flat, square controls with a high-contrast black, white and gray palette. Decorative CSS shadows and background gradients were removed. User-controlled artwork, signal displays and actual shadow/projection mechanisms remain part of their product function.

All forty scripts parsed and initialized in a mocked DOM after localization. The five standalone HTML exporters for Motion Type, Exhibition Builder, Explanation Studio, Mini Game Engine and Micro Book produced English documents; their embedded scripts also parsed. The English twelve-word composition met its three-line constraint, cipher shifting round-tripped, escape-room choices and ordering answers matched their translated story, and the bounded drawing interpreter retained its ninety-six-segment example. These are local structural/logic checks, not a real browser recording, GPU or visual-accessibility audit.
