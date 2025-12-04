**1.2.0**

* Updated for Alloyed Collective to add the new drones.
* Improved the logic so that this will work with other AISkillDrivers with CustomTarget.
  * This means it works properly with the Emergency and Transport drones. Either reset your config or remove the Emergency Drone from the blacklist for this to take effect.
* Added compatibility with [DroneRecycler](https://thunderstore.io/package/Chinchi/DroneRecycler/) so Equipment Drone doesn't need to be blacklisted either.
  * A side effect is that if you have ordered the drone to recycle a pickup and an implosion occurs, the drone will drop the recycle command and try to avoid the implosion. You'll just have to issue another command after the fact, but this is a small price to pay for the overall survivability of the drone.

**1.1.0**

* Added Risk of Options compatibility.
* Fixed a hook that was broken with Seekers of the Storm. That was a long time ago but forgot to make a release, oops!

**1.0.1**

* Update for Seekers of the Storm.

**1.0.0**

* Release