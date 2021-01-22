# asterax

# Description

Asteroids is a simple 2D space shooter made in Unity.

It contains a few problems, and has the Backtrace Unity SDK build in so the game problems can be reported on.

The four exceptions/problems:
- [Asteroid.cs](Assets/__Scripts/Asteroid.cs) when an asteroid is hit by a bullet, a nullreference is thrown, captured and sent to Backtrace
- [Asteroid.cs](Assets/__Scripts/Asteroid.cs) when the player ship's health goes down to 0, a native exception is triggered
- [AsteroidSpawner.cs](Assets/__Scripts/AsteroidSpawner.cs) upon spawning the asteriods at game startup, it keeps creating textures, causes memory pressure, and then clearing the textures to prevent the application from being repead by the OS, and keeps looping. Mobile only.
- [Bullet.cs](Assets/__Scripts/Bullet.cs) when a bullet is destroyed without hitting an asteroid, a divide by zero is triggered.
- [OffScreenWrapper.cs](Assets/__Scripts/OffScreenWrapper.cs) when the ship hits the edge of the screen, a call is done to a slow webservice in the main thread - triggering a hang.
- [PlayerShip.cs](Assets/__Scripts/PlayerShip.cs) when there's more than 2 bullets in flight, an InsuffMemory is thrown (preventing more bullets from being created)
- [PlayerShip.cs](Assets/__Scripts/PlayerShip.cs) when the player has less than 3 bullets available, a read to an not existing file is done

# References

Thanks to [Jeremy Gibson Bond](https://www.coursera.org/lecture/core-interaction-programming/challenge-1-scripting-needs-Vahew) for his coursera course.

