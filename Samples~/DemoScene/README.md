# Pollinations Unity demo

1. Import this sample, then open `DemoScene.unity`. It contains a camera and a
   `PollinationsDemo` object; the overlay UI builds itself at runtime.
2. In Play mode, set your publishable `pk_...` App Key on `PollinationsDemo`.
   Right-click the component header, choose **Connect Pollen**, then approve the
   code shown in the status text or Console. For editor-only testing, you can
   instead set a development key via **Tools → Pollinations → Development API Key**
   or `POLLINATIONS_API_KEY`; it is not serialized into the scene or build.
3. Choose **Run Pollinations demo** from the same menu.

The demo generates a one-sentence scene description, a 512x512 image of the prompt
(shown in the RawImage), and spoken audio of the sentence (played by the AudioSource).

The sample keeps the player device token in memory only. Shipped games should
not serialize device tokens or development keys into scenes, prefabs, or builds.
