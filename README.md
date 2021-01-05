2020-jan-04 - all of those 4 version are the same thing but tiny things that i had added on with help from andrej benedik for a couple things. version 0.40 isn't working. i will upload the one that works with void expanse soon.

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
