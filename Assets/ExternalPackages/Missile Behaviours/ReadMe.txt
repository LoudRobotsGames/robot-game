This package contains several C# scripts from three different categories:

	1. Controller:
		The controller category contains three different scripts. The MissileController.cs, which handles everything from acceleration to fuel consumption, and two guidance scripts.
		The guidance scripts rotate the missile via the MissileController in order to hit a target.

	2. Trigger:
		Trigger scripts fire an event if/when a certain condition is met. 

	3. Actions:
		Action scripts can react to one or more events and then perform an action. Under certain conditions they will also fire an event themselves, which can then trigger another action script.

The scripts:
	
	Control scripts:
		- MissileController.cs:
		  The missile controller contains a reference to the target of the missile, controls the rotation rate and speed of the missile and handles fuel consumption.
		  It has several properties you can use to get useful information like its maximum speed, how long its fuel will last or wether or not it's currently accelerating.
		  Addionally you can set a delay for the acceleration. The missile will not burn fuel or accelerate until that delay is over.
		  You can also give the missile an inital impulse, which will be applied as soon as the missile is created.

		- ProNavGuidance.cs:
		  This script calculates a rotation and uses the missile controller to rotate the missile towards that rotation in order to create an intercept course to the target.

		- TailChaseGuidance.cs:
		  This script will rotate the missile towards the target, using the missile controller. This results in what is called "tail chase". Which means the missile is likely to miss the target
		  at first, swing back and then tail it. Sometimes it even misses the second approach. This way is far less effectie than proportional navigation because it has a lower chance to hit and
		  will usually take much longer to even reach the target.

	Trigger scripts:
		- OnCollision.cs
		  This script will trigger if the gameobject its attached to collides with something. Note that it has its own layer mask to filter out collisions.

		- OnDistanceToTarget.cs
		  This script requieres a missile controller in order to get access to its target. It will trigger once the gameobject comes within a certain distance of the target.

		- OnHeight.cs
		  This script will trigger if the gameobject its attached to reaches a certain height.

		- OnProximity.cs
		  This script will trigger if the gameobject its attached to comes close to any object on any of the specified layers.

		- OnTargetLost.cs
		  This script requieres a missile controller in order get access to its target. It will trigger once that target is set to null, which will happen if the target gets destroyed or
		  if its set to null manually.

		- OnTime.cs
		  This script will trigger after a certain amount of time. It can trigger repeatedly if that option is ticked.
		  
		- TriggerBase.cs
		  The base class for all triggers.
		  
	Action scripts:
		- Explode.cs
		  Once triggered this script instantiates an explosions prefab, trigger an event and then destroys the gameobject its attached to. This can prevent other scripts from finishing.
		  Make sure the trigger order is correct, so this doesn't happen. You can also set a delay for the explosion to make sure everything else happens first. Any value bigger than 0 will make
		  sure that the destruction of the gameobject is delayed by one frame.
		  
		- SearchForNewTarget.cs
		  Once triggered this script will search the specified area for a new target. If it can not find a new target, it will trigger an event. 

		- SpawnSwarm.cs
		  Once triggered this script will instantiate a number of prefabs in a radial pattern around the gameobject its attached to.
		  It can then pass on its target and its velocity to these objects. Take a look at the comments in the script to learn more.

		- Disperse.cs
		  Once triggered this script will rotate the missile by a random amount on its own forward axis. After that it will randomly rotate the missile on its up axis.
		  This creates the illusion of random movement. When used with several missiles at the same time, the 'dispersion effect' becomes visible.

That all of these scripts use the MissileBehaviour namespace as a basis. This makes it easy to add any scripts to your gameobjects by using the AddComponent menu.

Note: The demo scene uses custom layers: 'Ground', 'Target' and 'Missiles' and now new, 'Dreadnought'. Unity will not create these when the package is imported.
Instead Unity uses an ID code for each layer, so every prefab and every layer mask will still point to these IDs.
If you import this into a project with already setup layers it will point to those.
In an empty scence these IDs will still point to different layers, but you will not be able to identify them because they will have no names.
Everything will still work as normal though, but this might make it hard to figure out the inner workings of the demo scene.
If you do not have any layers setup I suggest you add the following layers to your project:
Layer 8: Missiles
Layer 9: Targets
Layer 10: Ground
Layer 11: Dreadnought (For the second demo only)

Additionally, Unity does not save the physics settings. That means things that shouldn't be able to collide, now can. I suggest disableing Missile-Missile collision and (for the Dreadnought demo)
the Dreadnought-Missile collision. Otherwise things may not work as intended.

Also note that most of the ressources used in the demo scene are part of the Unity standard assets.

If something is unclear, I suggest takeing a look at the demo scene to see how it's done there.

I hope you enjoy this asset and if you find any bugs, have questions or suggestions please email me at sentrigal@gmail.com.