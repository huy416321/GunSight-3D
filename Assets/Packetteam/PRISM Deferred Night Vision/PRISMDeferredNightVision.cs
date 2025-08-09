using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Experimental.Rendering;

namespace Prism { 
[System.Serializable, VolumeComponentMenu("PRISM Post Processing/PRISM Deferred Night Vision")]
/// <summary>
/// Deferred night vision effect.
/// </summary>
public class PRISMDeferredNightVision : VolumeComponent
{
	[Header("To disable the effect, simply set all overrides to disabled, or click 'None'")]
	[Space]
	[SerializeField]
	[Tooltip("The main color of the NV effect")]
	[Header("The main color of the NV effect")]
	public ColorParameter m_NVColor = new ColorParameter(new Color(0f,1f,0.1724138f,0f));

	[SerializeField]
	[Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
	[Header("The color that the NV effect will 'bleach' towards (white = default)")]
	public ColorParameter m_TargetBleachColor = new ColorParameter(new Color(1f, 1f,1f,0f));

	[Tooltip("How much base lighting does the NV effect pick up")]
	[Header("How much base lighting does the NV effect pick up")]
	public ClampedFloatParameter m_baseLightingContribution = new ClampedFloatParameter(0.025f,0f,0.1f);

	[Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
	[Header("The higher this value, the more bright areas will get 'bleached out'")]
	public ClampedFloatParameter m_LightSensitivityMultiplier = new ClampedFloatParameter(100f,0f,128f);

	Material m_Material;
	Shader m_Shader;

	[Tooltip("Do we want to apply a vignette to the edges of the screen?")]
	[Header("Apply a vignette to the edges of the screen?")]
	public BoolParameter useVignetting = new BoolParameter(true);
}

}