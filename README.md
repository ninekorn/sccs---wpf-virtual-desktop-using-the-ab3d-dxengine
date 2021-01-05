2021-jan-04 - all of those 4 versions are the same thing. I was able to achieve this in 2018 with help for tons of things from andrej benedik. draft version 0.40 isn't working currently anymore but the other 3 should work.
Ive had correspondances with andrej benedik from ab4d.com which helped me a lot at the same time as i was working on trying to make a C# ab3d.dxengine virtual desktop work for void expanse. This repo and my correspondances with him would make for a very good tutorial in its entirety on how to make a virtual desktop work with windows 10 osk.exe keyboard included and mouse clicks from the oculus touch working inside of the ab3d dxengine.
Releasing my drafts and hobbyst programmer work so late is a mess though, but i wanted everything to work at the same time when i was going to release my void expanse 9smods with virtual reality and a virtual desktop... it needed to have the virtual desktop working in both void expanse and elite dangerous and the controls and a "simili VR cockpit or minecraft VR kinda way to play minecraft in vr in windows 10"...  sometimes shit happens and sometimes it happens to us more often than others. Sometimes i feel like i failed the atomic torch studios for me to be a succesfull modder for video games and bringing void expanse in virtual reality playable on a cheap DIY virtual desktop back when the oculus rift cv1 was still popular in 2018...  As long as i will live, i will work towards achieving windows 10 voice recognition working for void expanse inside of virtual reality using the normal
Void Expanse API but for the moment i would only be able to achoeve this with the oculus rift cv1. I know of only one way to do it...

Currently in version 0.41, only the ship zoom and fire buttons are mapped. I will work soon on mapping the rest of the controls for a version 0.42, when i will have the time to code. 

Thank you for reading me.
steve chassé


2021-jan-02-
i wanted this to have the 60 days trial using the ab3d.dxengine.OculusWrap, right out of the box, for people to use the ab3d.dxengine.oculusWrap. I modified my repos so you won't be able to make them work out of the box anymore.

You will have to go clone the repository here https://github.com/ab4d/Ab3d.OculusWrap and build the dlls separately for yourselves. If the ab3d.dxengine.oculusWrap would be provided in the future with a nugget you won't have to do those steps. 

1. Clone the github repository here: https://github.com/ab4d/Ab3d.OculusWrap
2. Build the ab3d.OculusWrap solution first with the FrameWork 4.5 or 4.7.2 whatever.
3. Then build the solution ab3d.DXEngine.OculusWrap.
4. use both the ab3d.OculusWrap.dll and the ab3d.DXEngine.OculusWrap as references for my projects sc_core_systems and SCCoreSystems and the solution sccsv10 and this one sccsv11 and that one too sccsVD4VE. Those DLLs will make the projects work. after inserting those references, rebuild your projects and this should take care of restoring the nugget packages for the other dlls to load.

thank you for reading me,
steve chassé

2020-dec-25-

# sccs-wpf-virtual-desktop-using-the-ab3d-dxengine

i will be releasing 4 versions of it in this repository today if i have the chance. 
