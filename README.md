# Pollinations for Unity

A small, dependency-free Unity Package Manager client for [Pollinations](https://pollinations.ai)
text, image, and speech generation, with live model lists and player-funded device-flow
authorization. Submitted for Pollinations quest #15580.

- Async C# helpers for text, image, and speech generation (`Task`-based, `CancellationToken` on every call)
- Player-funded auth through the [BYOP device flow](https://github.com/pollinations/pollinations/blob/main/BRING_YOUR_OWN_POLLEN.md), plus a development API key for the editor
- Any model from the live [text](https://gen.pollinations.ai/text/models), [image](https://gen.pollinations.ai/image/models), and [audio](https://gen.pollinations.ai/audio/models) model lists
- A ready-to-run sample scene
- No third-party dependencies: only `UnityWebRequest`, `JsonUtility`, `Texture2D`, and `DownloadHandlerAudioClip`

## Install

Unity 2021.3 or newer. Open **Window → Package Manager → + → Add package from git URL…** and paste:

```text
https://github.com/Marcus-Mok-GH/pollinations-unity.git
```

Or add the line below to the `dependencies` block of your project's `Packages/manifest.json`:

```json
"com.pollinations.unity": "https://github.com/Marcus-Mok-GH/pollinations-unity.git"
```

## Quick start

```csharp
using Pollinations.Unity;

var client = new PollinationsClient { ApiKey = "sk_your_development_key" };

var text = await client.TextAsync("Give my NPC one cheerful sentence.");
var image = await client.ImageAsync("a cozy pixel-art tavern", width: 512, height: 512);
var speech = await client.SpeechAsync(text.Text);

rawImage.texture = image.Texture;
audioSource.clip = speech.Clip;
audioSource.Play();
```

Get a development key at [enter.pollinations.ai/keys](https://enter.pollinations.ai/keys).
Never commit it or serialize it into a scene, prefab, or build.

## Short demo (sample scene)

1. Import the sample: **Window → Package Manager → Pollinations for Unity → Samples → Demo Scene → Import**.
2. Open `Assets/Samples/Pollinations for Unity/Demo Scene/DemoScene.unity`. It contains a camera and a `PollinationsDemo` object; the overlay UI (RawImage + status text) builds itself at runtime.
3. In Play mode, set a publishable App Key on the component and choose **Connect Pollen** from its context menu. Approve the code in your browser. Alternatively, for editor-only development, set a key via **Tools → Pollinations → Development API Key** or `POLLINATIONS_API_KEY`.
4. Choose **Run Pollinations demo** from the component context menu (or call `Run()` from code).

Expected result, in order:

- Console: `[Pollinations] Generating text...` then a one-sentence scene description
- The square RawImage fills with a generated 512x512 image of the prompt
- The sentence is spoken through the AudioSource
- Console: `[Pollinations] Done - the sentence was also spoken.`

## API coverage

| API | Client method | Endpoint |
| --- | --- | --- |
| Text | `TextAsync` | `POST https://gen.pollinations.ai/v1/chat/completions` |
| Image | `ImageAsync` | `POST https://gen.pollinations.ai/v1/images/generations` |
| Speech | `SpeechAsync` | `POST https://gen.pollinations.ai/v1/audio/speech` |
| Live models | `PollinationsModels.FetchAsync` | `GET /text/models`, `/image/models`, `/audio/models` |
| Player-funded auth | `PollinationsAuth.AuthorizeAsync` | `POST https://enter.pollinations.ai/api/device/code` and `/api/device/token` |

`ImageAsync` accepts both `b64_json` and URL image responses and returns a decoded
`Texture2D`. `SpeechAsync` returns a ready-to-play `AudioClip` (WAV, MP3, OGG).
API errors surface the provider's own error message whenever the response carries one.

## Player-funded device flow

For a shipped game, create a publishable `pk_...` App Key and let each player authorize
with their own Pollen at runtime:

```csharp
var auth = new PollinationsAuth("pk_your_app_key");
var result = await auth.AuthorizeAsync(progress =>
    statusText.text = progress.Message);

if (result.State == DeviceFlowState.Granted)
    client.DeviceToken = result.AccessToken; // keep in memory only
```

The flow handles `authorization_pending`, `slow_down`, `access_denied`, and `expired_token`,
and follows the [BYOP documentation](https://github.com/pollinations/pollinations/blob/main/BRING_YOUR_OWN_POLLEN.md).
Device-flow tokens stay in memory; do not serialize them or place development keys in a
scene, prefab, or build.

## Tests

The package includes Unity Test Runner coverage for credential hygiene, App Key validation,
and request JSON shape. Run them with **Window → General → Test Runner**.

## License

MIT
