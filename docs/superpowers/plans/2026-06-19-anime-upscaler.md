# Anime Upscaler Desktop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a portable, fully offline Windows anime upscaler with V1/V2/V3 models and a before-after comparison slider.

**Architecture:** A testable .NET core library controls the packaged Real-ESRGAN NCNN Vulkan process. A WPF shell handles selection, drag-and-drop, comparison, progress, cancellation, and saving while bundled assets provide local inference.

**Tech Stack:** .NET 8, WPF, xUnit, Real-ESRGAN NCNN Vulkan, NCNN model weights.

---

### Task 1: Core model catalog and command contract

- [ ] Write failing tests for model-scale availability, format mapping, argument construction, and percent parsing.
- [ ] Run the tests and confirm the missing production types cause the expected failure.
- [ ] Implement immutable model definitions and an engine request/command builder.
- [ ] Run the tests and confirm they pass.

### Task 2: Offline process runner

- [ ] Write failing tests for asset validation and error translation.
- [ ] Implement asynchronous process execution, output parsing, cancellation, and process-tree termination.
- [ ] Run the complete core test suite.

### Task 3: WPF comparison interface

- [ ] Build the dark desktop shell with input drop zone, model/scale/format controls, status, progress, and actions.
- [ ] Implement file loading, after-image clipping, drag-and-drop, save-as, cancellation, and temporary-file cleanup.
- [ ] Build the WPF project with warnings treated as errors.

### Task 4: Package engine and models

- [ ] Copy the official Windows engine and runtime DLL into application assets.
- [ ] Copy and rename the V1/V2/V3 model files into isolated per-model directories matching the engine contract.
- [ ] Include upstream and model-source license/attribution notices.
- [ ] Publish a self-contained Windows x64 folder.

### Task 5: End-to-end verification

- [ ] Run all unit tests.
- [ ] Smoke-test every valid model-scale pair using a generated input image and validate output dimensions.
- [ ] Launch the packaged WPF executable and verify the main window remains running without startup errors.
- [ ] Record usage and build instructions in the README and commit the verified result.

