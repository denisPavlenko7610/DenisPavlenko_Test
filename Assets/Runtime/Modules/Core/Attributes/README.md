# Attributes

Three optional Inspector helpers:

- `[Assign]` fills an empty component field from the same object, a parent, a child, or one unique component in the same scene.
- `[Button]` shows a button for a parameterless method.
- `[Scene]` stores a scene asset as its path in a string field.

`Self`, `Parent` and `Children` are distinct scopes: parent/child searches do not fall back to the current object. `Scene` includes inactive objects, never crosses into another loaded scene and leaves the field empty when the match is ambiguous.

Copy this folder with its asmdef. Runtime attributes compile into `UnityTemplates.Attributes`; drawers compile into the Editor-only `UnityTemplates.Attributes.Editor`. `Examples` is optional and its shared UI depends on `UnityTemplates.Foundation.Examples`.

These helpers improve authoring only. Components should still validate required references at runtime.

Open `../../../Examples/Core/Attributes/AttributesExample.unity` and select `Module Example`. Its `ShipDebugTools` component demonstrates all three attributes on a small repair workflow; the runtime UI calls the same ordinary methods.
