# Match More

A performance-optimized, mobile-ready Match-3 puzzle game developed with Unity. This repository demonstrates scalable game architecture, clean code practices, and mobile optimization techniques.

## Architecture & Design Patterns

The codebase is built with maintainability and scalability in mind, utilizing several core software engineering patterns:

*   **Event-Driven Architecture:** A centralized `EventBus` handles decoupled communication across the game. Systems like UI, Audio, and Gameplay react to state changes (e.g., `GoldChanged`, `LevelChanged`, `ScoreChanged`) without hard dependencies on each other.
*   **Model-View-Controller (MVC) Approach:** The core board mechanics strictly separate data (`BoardModel`), visual representation (`BoardView`), and input/flow control (`BoardController`).
*   **Service Locator:** Core dependencies such as `ISaveService` and `ICurrencyService` are initialized and accessed via a centralized `Services` locator, avoiding singleton abuse and tightly coupled classes.
*   **Object Pooling:** Heavy instantiation operations (like generating and destroying tiles) are optimized using a custom generic `ObjectPool`. This ensures zero garbage collection spikes and stable frame rates during cascades.

## Core Systems

*   **Input Management:** Built on Unity's new Input System utilizing `EnhancedTouchSupport`. Input flow is strictly regulated by an `InputGate` to prevent race conditions and unintended player actions during critical animations.
*   **Match Detection Algorithm:** The `MatchFinder` class features a matrix-scanning algorithm capable of recognizing complex shapes, including horizontal/vertical lines, T/L intersections, and 2x2/3x2 block matches.
*   **Save & Economy System:** A robust JSON-based local serialization system manages player persistence, storing progression constraints, currency (Gold), and audio preferences.
*   **Animation & UI Sequencer:** Extensive integration of DOTween for high-performance UI sequencing (e.g., bezier-curve coin flight animations in `GoldFlyAnimation`) and seamless gameplay interpolations.

## Technical Specifications

*   **Engine Version:** Unity 6000.3.9f1
*   **Target Platform:** Android (Optimized for ARM64 architecture)
*   **Scripting Backend:** IL2CPP
*   **Key Dependencies:** DOTween, Unity Input System, TextMeshPro

## Build and Run

1. Open the project in Unity 6000.3.9f1.
2. Navigate to `File -> Build Profiles`.
3. Select the Android platform.
4. Ensure `Boot` is initialized as index 0 in the Scenes in Build list.
5. Click Build and Run to deploy to a connected Android device.
