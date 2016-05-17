Hello! Thank you for the purchase. Hope the assets work well.
If they don't, please contact me slsovest@gmail.com.




Assembling the robots:

Legs, shoulders and cockpits contain containers for mounting other parts, their names start with "Mount_".
Just drop the part in the corresponding container, and It'll snap into place.

- Start with legs. 
- The first container is in Legs->HIPS->Pelvis->Top->Mount_top. 
- Put the shoulders or the cockpit into "Mount_top".
- Find other containers inside shoulders and cockpit.

After the assembly, robots consist of many separate parts and, even with batching, produce high number of draw calls.
You may want to combine non-animated parts into a single mesh for the sake of optimization.

All weapons contain locators at their barrel ends (named "Barrel_end"). Rocket launchers contain multiple locators, for all of the rockets.

If you need a cap for the hole at the top of the legs, you can find it in the Models->Legs_top_cap.fbx. Just drop it in Legs->HIPS, then move from HIPS to Pelvis.




Animations:

Idle and Jump_jetpack animation files contain several animations:
Idle: "Idle_simple" - frames 170-260
Jump_jetpack: "Jump_jet_start"(6-14), "Jump_jet_idle" - 15-55", "Jump_jet_land" - 56-67

Unlike the other weapons, the minigun hasn't the same animation for all levels (due to different barrel rotation speed), check its Animator Controller.
Jetpack Jump
Switch to "jump_idle" after the "jump_start". You may want to tilt the the mech forward or backward when mech is flying. Switch to "jump_land" when it's time to land.
Simple jump
Consists of only one animation, not as flexible as the Jetpack jump. Hope it still could be useful in some projects.

If you want mech weapons to bounce a bit while running or walking,
you may drop the "Top_anim_weapons_bounce" prefab into the top part after mech is assembled, and drag the weapons into "Top_anim_weapons_bounce/Mount_weapons" container.

If you want to tweak the animations or create the new ones, the source .ma file contains the animated parts with their rigs.




The texture PSD:

For a quick repaint, adjust the layers in the "COLOR" folder. You can drop your decals and textures (camouflage, for example) in the folder as well. Just be careful with texture seams.
You may want to turn off the "FX_Rust" and "FX_Chipped_paint" layers for more cartoony look.
Or make ambient occlusion stronger by increasing opacity of "SHADING/MORE_OCCLUSION" layer.




I'm fairly new to Unity, and if you have any ideas how I could organize the assets any better way, please, write.
I'm planning to develop the Mech Construstor assets further. The images of the progress will be added here: https://www.behance.net/slava_zhuravlev/wip
If you have any ideas for the ongoing assets, if you think of a certain module or a weapon type which should be made, please write me via slsovest@gmail.com.
I will try to include it in the future assets.




Version 1.1

Added skinned legs, cannons and machineguns. The models consist only of a single mesh object, and require 1 draw call (instead of 12 for the old legs, my bad). But do not batch.
Replaced the old prefabs with the new ones. The old ones can be found in Prefabs/Non skinned folder.
They may still work better (because of batching), if you have a lot of robots in a scene.


Version 1.2

Replaced in-place legs animations with animations with root motion. Hope they will work fine with Mecanim. If not, please, write. 
The old animations are in Animations/Legs_In_place_animations.rar.
The legs pivot was slightly off center, fixed. (Thanks to Devon G. for pointing to that.)


Version 1.3

4 new parts added:
- Shoulders_lt_frame_upgrade
- Shoulders_med_frame_upgrade
- Shoulders_med_shield_upgrade
- Cockpit_jet_upgrade

New animations added:
- Jetpack jump
- Simple jump
- Fall


Version 1.4

Root motion finally fixed (had to add an additional parent bone to all the legs and animations).
Walk and run animation tweaked a bit.
Turn on place animation added.
Added HalfShoulder parts (can be very useful with the Spiders and Tanks pack).