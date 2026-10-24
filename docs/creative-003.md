# Creative studio contracts, batch 003

The products in this batch perform more complete authoring workflows rather than adding visual themes to previous tools. Each owns its source and can be opened independently.

| Product | Implemented mechanism |
| --- | --- |
| Video Kurgu | Multiple local video sources are trimmed into timeline clips with speed and captions. Canvas rendering and the user's media-element audio graph are combined into a MediaStream. A supported WebM recorder captures actual playback. The recorder pauses while each next source seeks, and final data is collected before download. Unlike the audio workshop, it edits moving picture content. |
| Düğüm Gölgelendirici | A directed math graph generates GLSL expressions after recursive dependency/cycle checks. Actual WebGL vertex/fragment compilation and program linking are checked before replacing the previous program. PNG, GLSL and editable graph outputs serve different authoring uses. |
| Hareketli Yazı | Text tracks store independent transform/opacity keyframes. Linear, smooth and overshooting interpolation drive a timeline. Exported HTML contains the model and its standalone playback engine; it does not depend on the editor. |
| Müzik Haritası | A listener traverses spatial note nodes, recording actual visit times. Node x-position sets stereo placement. Offline sample synthesis writes an actual signed stereo PCM WAV from the recorded route, unlike either drum sequencing or radial subdivision. |
| Gölge Sahnesi | A point-light projection expands puppet coordinates from their depth onto the screen plane. Two actor poses interpolate; light size controls a soft-edge approximation. The layout view is separate from the projected shadow output. |
| Dünya Dokumacı | Seeded interpolated noise at several scales produces elevation/moisture; the user then brushes terrain and names settlements. Terrain-weighted A* finds land-only roads. Water barriers may leave islands disconnected. Editable world data is separate from flattened PNG output. |
| Mikro Kitap | Canvas text metrics wrap a manuscript into bounded lines and pages. Cover and page content can be edited; standalone HTML reads and prints without the editor. This produces a multi-page document rather than a single poster. |
| Sergi Kurucu | Authored text/color/motif exhibits occupy a navigable 2D room. A self-contained visitor engine handles movement and nearby descriptions. HTML export embeds the data and engine; it does not pretend the portfolio directory is the exhibition. |
| Sahne Akışı | Cue records schedule tones, light transitions, captions and blackouts on a virtual stage. Automatic time and manual GO are distinct playback modes. A real CSV exports the authored cue sheet, with no claim of physical lighting control. |
| Kat İzleri | Four connected panels are folded by cumulative hinge transforms into ten 3D vertices and eight triangles. Projection is a view of the actual mesh; OBJ exports those same vertices/faces. Unlike Kâğıt Katla, it computes folded geometry rather than a flat box net. |

## API implementation decisions

The video export follows the browser contracts described in [canvas captureStream](https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement/captureStream), [MediaRecorder type support](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder/isTypeSupported_static), and [the final dataavailable event](https://developer.mozilla.org/en-US/docs/Web/API/MediaRecorder/dataavailable_event). These are documentation-backed contracts, not proof of a successful recording on every browser. The implementation chooses an available WebM format, records in real time, and downloads only after stop has delivered the final data. Hidden-tab cancellation prevents describing background-throttled export as complete.

WebGL uses the actual [compileShader API](https://developer.mozilla.org/en-US/docs/Web/API/WebGLRenderingContext/compileShader). It checks compile/link status and keeps the active vertex buffer alive with the program. A cyclic graph leaves the previous successful image in place and reports the graph error.

## Execution evidence

The first ten studio scripts parse in Node.js. The spatial composition WAV writer was executed with stereo samples; its channel count, byte length and interleaved signed sample values matched the expected container. Folded-mesh execution preserved every 200-unit hinge and 100-unit panel edge. The terrain pathfinder found a short land path and returned no path across a complete water barrier. This does not establish native video codec support, actual GPU compilation, media permissions or perceptual audio quality.
