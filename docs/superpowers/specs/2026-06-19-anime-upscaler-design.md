# Anime Upscaler Desktop Design

## Goal

Build a Windows 10/11 desktop application that upscales one anime image fully offline using Vulkan acceleration and presents the original/result through a before-after comparison slider.

## Architecture

The application is a .NET 8 WPF GUI written from scratch. It invokes the official MIT-licensed Real-ESRGAN NCNN Vulkan Windows engine as a child process and packages compatible anime model weights separately; no Android application code is copied. A small core library owns the model catalog, command construction, process lifecycle, progress parsing, cancellation, and validation so these behaviors are testable without WPF.

## Models and scaling

- Anime V1: 4x.
- Anime V2: 2x and 4x.
- Anime V3: 2x, 3x, and 4x.
- The scale selector only offers scales supported by the selected model.
- Models run locally from packaged `.param` and `.bin` files. The application performs no network requests.

## User experience

- Input is selected with a file picker or drag-and-drop; PNG, JPG/JPEG, and WebP are accepted.
- The main canvas places the processed result below the original. A horizontal comparison slider clips the original layer and renders a bright divider exactly on the boundary.
- The comparison canvas supports 100%–400% zoom through buttons and Ctrl+mouse-wheel scrolling.
- Controls select model, scale, and PNG/JPG/WebP output. PNG is the default.
- Upscaling shows progress and elapsed time, can be cancelled, and keeps the source image unchanged.
- A successful result can be saved with a file picker. Temporary results are cleaned up when replaced or the app closes.

## Failure behavior

Missing assets, unsupported files, invalid model-scale combinations, Vulkan/driver failures, engine crashes, and write failures produce Indonesian messages. Engine cancellation kills the process tree. Controls that would create conflicting work are disabled while processing.

## Distribution and attribution

Publish as a self-contained Windows x64 folder containing the GUI, Real-ESRGAN NCNN Vulkan executable/runtime, model files, and third-party license notices. The package requires no Python, CUDA, account, server, or internet connection. The UI uses the Lumina visual identity; the word Anime is reserved for model names only.

## Verification

Automated tests cover the model catalog, scale validation, safe command arguments, progress parsing, output extension selection, and missing assets. Build verification publishes the self-contained app. A smoke test runs every packaged model-scale combination against a small local image and confirms the expected output dimensions.
