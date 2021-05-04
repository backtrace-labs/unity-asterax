# asterax

# Live demo

- [WebGL version](https://backtrace-labs.github.io/unity-asterax/)

# Description

Asteroids is a simple 2D space shooter made in Unity.

It contains a few problems, and has the Backtrace Unity SDK build in so the game problems can be reported on.

The exceptions/problems, spread across the application (to create realistic stacks):
- [Asteroid.cs](Assets/__Scripts/Asteroid.cs) when an asteroid is hit by a bullet, a nullreference is thrown, captured and sent to Backtrace
- [Asteroid.cs](Assets/__Scripts/Asteroid.cs) when the player ship's health goes down to 0, a native exception is triggered
- [AsteroidSpawner.cs](Assets/__Scripts/AsteroidSpawner.cs) upon spawning the asteriods (which is done every 500ms), when the number of asteroids reaches 50% of the maximum, the game will keep creating textures, causes memory pressure, and then clearing the textures to prevent the application from being repead by the OS, and keeps looping. Mobile only.
- [OffscreenWrapper.cs](Assets/__Scripts/OffscreenWrapper.cs) when the player leaves the left or right side of the side, a divide by zero is triggered.
- [Asterax.cs](Assets/__Scripts/Asterax.cs) when the player hits a score of 100, a call is done to a slow webservice in the main thread - triggering a hang. Mobile only.
- [Asterax.cs](Assets/__Scripts/Asterax.cs) when the player hits a score divisable by 150, an attempt will be made to clear all Asteroid objects from a rogue thread - this fails on most platforms. On the platforms it doesn't, a NullReference is triggered.
- [Asterax.cs](Assets/__Scripts/Asterax.cs) when the player hits a score divisable by 200, all Asteroid objects will be cleared and a `level_completed` session event will be triggered.
- [PlayerShip.cs](Assets/__Scripts/PlayerShip.cs) when the player has less than 3 bullets available, a read to an not existing file is done

# References

Thanks to [Jeremy Gibson Bond](https://www.coursera.org/lecture/core-interaction-programming/challenge-1-scripting-needs-Vahew) for his coursera course.

