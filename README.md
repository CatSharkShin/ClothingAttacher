Made so that my customers can attach their new clothes easily in Unity.
https://catsharku.gumroad.com

# Usage
1. Import the [ClothingAttacher.dll](https://github.com/CatSharkShin/ClothingAttacher/releases/download/v1.0.0/ClothingAttacher.dll).
2. Drag your clothing into the hierarchy.
<br>
<img
  src="https://i.imgur.com/HWzKScD.gif"
  alt="Alt text"
  title="Optional title"
  style="display: block; margin: 0 auto;  width: 300px">
3. Unpack the clothings Prefab
<img
  src="https://i.imgur.com/CEwVqSH.gif"
  alt="Alt text"
  title="Optional title"
  style="display: block; margin: 0 auto;  width: 300px">
4. Drag the clothing on your avatar
<img
  src="https://i.imgur.com/wVpo9K4.gif"
  alt="Alt text"
  title="Optional title"
  style="display: block; margin: 0 auto;  width: 300px">
5. In the Inspector, click "Attach" under the "SkinnedMeshRenderer" Component
<img
  src="https://i.imgur.com/CanKzRc.png"
  alt="Alt text"
  title="Optional title"
  style="display: block; margin: 0 auto;  width: 300px">
  
# What does it actually do?
Well the code is pretty easy to read but I will elaborate so that you know what will happen to your bones and stuff
- It finds the "Armature" gameobject, then looks at all the bones listed in your clothing, and tries finding them under the "Armature"
- If it can't find the bone, it will fix it by finding a bone-parent that it can actually find under the armature, and then adds the missing bone.
