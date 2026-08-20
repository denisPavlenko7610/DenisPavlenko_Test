# Ball Shot Prototype

## Gameplay Requirements

The screen contains a player ball in the lower-left corner and a target in the upper-right corner. The player ball must reach the target. Obstacles block the route. The player ball creates shots by transferring part of its own volume into them. The player must clear the route so the ball can hop along the cleared path to the final target.

## Prototype Behaviour

When the player taps and holds, a shot ball starts separating from the player ball. As the shot grows, the player ball shrinks proportionally because its volume is transferred to the shot. Shot size depends on hold duration: the longer the input is held, the larger the shot becomes.

When the input is released, the shot travels towards the target. On contact with the first obstacle, it infects obstacles within its effect radius and causes them to explode.

Shot power is represented by the infection radius and depends on shot size. A larger shot has a larger infection radius. Obstacles placed close together are easier to infect with one shot. Small shots should be used for isolated obstacles so the player does not waste all of the player ball's volume.

After an area has been cleared, the player ball advances towards the target. Its size is smaller near the end, but the cleared route must provide enough room for it to pass freely between the remaining obstacles while hopping along the centre of the track. The track narrows together with the player ball.

There must be a door at the end. It opens when the player ball comes within 5 metres of it.

The player loses if the input is held for too long and the entire usable volume of the player ball is transferred into the shot, reaching a chosen critical minimum size. The player also loses if the remaining volume is insufficient to create the shots required to clear the route.

At the start of the level, the player ball must contain enough volume to complete the level with a 20% reserve.
