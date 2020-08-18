using UnityEngine;
using UnityEngine.Rendering;
using Valve.VR;

//渲染额外摄像机，区分前景和背景
public class SteamVR_ExternalCamera : MonoBehaviour
{
	[System.Serializable]
    // externalcamera.cfg配置文件信息
	public struct Config
	{
		public float x, y, z;
		public float rx, ry, rz;
		public float fov;
		public float near, far;
		public float sceneResolutionScale;

		public float frameSkip;
		public float nearOffset, farOffset;
		public float hmdOffset;
		public float r, g, b, a;    // chroma key

		public bool disableStandardAssets;
	}

	public Config config;
	public string configPath;   //配置文件路径

	public void ReadConfig()    //读取配置文件信息
	{
		try
		{
			var mCam = new HmdMatrix34_t();     //3x4矩阵表示设备位置 旋转+平移
			var readCamMatrix = false;

			object c = config; //装箱
			var lines = System.IO.File.ReadAllLines(configPath);
			foreach (var line in lines)//处理每一行的配置信息
			{
				var split = line.Split('=');
				if (split.Length == 2)
				{
					var key = split[0];
					if (key == "m")     //矩阵形式配置文件
					{
						var values = split[1].Split(',');
						if (values.Length == 12)    //分解矩阵
						{
							mCam.m0 = float.Parse(values[0]);
							mCam.m1 = float.Parse(values[1]);
							mCam.m2 = float.Parse(values[2]);
							mCam.m3 = float.Parse(values[3]);
							mCam.m4 = float.Parse(values[4]);
							mCam.m5 = float.Parse(values[5]);
							mCam.m6 = float.Parse(values[6]);
							mCam.m7 = float.Parse(values[7]);
							mCam.m8 = float.Parse(values[8]);
							mCam.m9 = float.Parse(values[9]);
							mCam.m10 = float.Parse(values[10]);
							mCam.m11 = float.Parse(values[11]);
							readCamMatrix = true;
						}
					}
#if !UNITY_METRO
					else if (key == "disableStandardAssets")    //禁用自带文件资源
					{
						var field = c.GetType().GetField(key);
						if (field != null)
							field.SetValue(c, bool.Parse(split[1]));
					}
					else
					{
						var field = c.GetType().GetField(key);  //配置文件的其他形式参数
						if (field != null)
							field.SetValue(c, float.Parse(split[1]));
					}
#endif
				}
			}
			config = (Config)c;  //拆箱

			// 把矩阵转换成位置和矩阵
			if (readCamMatrix)
			{
				var t = new SteamVR_Utils.RigidTransform(mCam);
				config.x = t.pos.x;
				config.y = t.pos.y;
				config.z = t.pos.z;
				var angles = t.rot.eulerAngles;
				config.rx = angles.x;
				config.ry = angles.y;
				config.rz = angles.z;
			}
		}
		catch { }

        // 清除目标，以便调用AttachToCamera以获取任何更改
        target = null;
#if !UNITY_METRO
		// 监听配置参数的变化
		if (watcher == null)
		{
			var fi = new System.IO.FileInfo(configPath);
			watcher = new System.IO.FileSystemWatcher(fi.DirectoryName, fi.Name);
			watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
			watcher.Changed += new System.IO.FileSystemEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
		}
	}

	void OnChanged(object source, System.IO.FileSystemEventArgs e)
	{
		ReadConfig();
	}

	System.IO.FileSystemWatcher watcher;
#else
	}
#endif
	Camera cam; //复制eye的相机
	Transform target;//头盔的Transform
	GameObject clipQuad;//四分屏
	Material clipMaterial;//材质

    //把本脚本和相机关联起来
	public void AttachToCamera(SteamVR_Camera vrcam)
	{
		if (target == vrcam.head)   //已经关联
			return;

		target = vrcam.head;    //target置为head

        // 将预制体与head同级
		var root = transform.parent;
		var origin = vrcam.head.parent;
		root.parent = origin;
		root.localPosition = Vector3.zero;
		root.localRotation = Quaternion.identity;
		root.localScale = Vector3.one;

		// 拷贝eye的camera并实例化.
		vrcam.enabled = false;
		var go = Instantiate(vrcam.gameObject);
		vrcam.enabled = true;
		go.name = "camera";


        
        // 删除里面的SteamVR_Camera及SteamVR_CameraFlip组件
        DestroyImmediate(go.GetComponent<SteamVR_Camera>());
		DestroyImmediate(go.GetComponent<SteamVR_Fade>());
        
		cam = go.GetComponent<Camera>();
		cam.stereoTargetEye = StereoTargetEyeMask.None;
		cam.fieldOfView = config.fov;   //设置相机FOV
		cam.useOcclusionCulling = false;    //去掉遮挡剔除



        //cam.enabled = false; //禁用
    


        // 带不同Shader的不同材质
        colorMat = new Material(Shader.Find("Custom/SteamVR_ColorOut"));
		alphaMat = new Material(Shader.Find("Custom/SteamVR_AlphaOut"));
		clipMaterial = new Material(Shader.Find("Custom/SteamVR_ClearAll"));

        // 将Camera设成Controller的子节点
        var offset = go.transform;
		offset.parent = transform;

        // 根据配置设定相对位置和旋转
        offset.localPosition = new Vector3(config.x, config.y, config.z);
		offset.localRotation = Quaternion.Euler(config.rx, config.ry, config.rz);
		offset.localScale = Vector3.one;

        // 去掉Camera下的所有子节点
        while (offset.childCount > 0)
			DestroyImmediate(offset.GetChild(0).gameObject);

  


        // 创建一个四边形用于裁剪，使用相机的裁剪会有阴影的问题
        clipQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		clipQuad.name = "ClipQuad";

        // 去掉MeshCollider
        DestroyImmediate(clipQuad.GetComponent<MeshCollider>());

        // 设置Render材质
        var clipRenderer = clipQuad.GetComponent<MeshRenderer>();
		clipRenderer.material = clipMaterial;
		clipRenderer.shadowCastingMode = ShadowCastingMode.Off; //不带阴影
		clipRenderer.receiveShadows = false;
		clipRenderer.lightProbeUsage = LightProbeUsage.Off;
		clipRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        // 将这个四边形设为Camera的子节点，
        var clipTransform = clipQuad.transform;
		clipTransform.parent = offset;
        // 放大
        clipTransform.localScale = new Vector3(1000.0f, 1000.0f, 1.0f);
		clipTransform.localRotation = Quaternion.identity;
        // 缺省禁用
        clipQuad.SetActive(false);
	}

    // 得到外部相机与head的距离
	public float GetTargetDistance()
	{
		if (target == null)
			return config.near + 0.01f;

		var offset = cam.transform;
		var forward = new Vector3(offset.forward.x, 0.0f, offset.forward.z).normalized;
		var targetPos = target.position + new Vector3(target.forward.x, 0.0f, target.forward.z).normalized * config.hmdOffset;

		var distance = -(new Plane(forward, targetPos)).GetDistanceToPoint(offset.position);
		return Mathf.Clamp(distance, config.near + 0.01f, config.far - 0.01f);
	}

	Material colorMat, alphaMat;

    //渲染前景和alpha
	public void RenderNear()
	{
        // 渲染的目标大小为屏幕的1/4
        var w = Screen.width / 2;
		var h = Screen.height / 2;

		if (cam.targetTexture == null || cam.targetTexture.width != w || cam.targetTexture.height != h)
		{
			var tex = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
			tex.antiAliasing = QualitySettings.antiAliasing == 0 ? 1 : QualitySettings.antiAliasing;
			cam.targetTexture = tex;
		}

        // 设置相机的远近裁剪面
        cam.nearClipPlane = config.near;
		cam.farClipPlane = config.far;

        //相机参数
		var clearFlags = cam.clearFlags;
		var backgroundColor = cam.backgroundColor;
		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = Color.clear;  // 背景设为黑色

		clipMaterial.color = new Color(config.r, config.g, config.b, config.a);
        //将裁剪平面设置到了head的位置
        float dist = Mathf.Clamp(GetTargetDistance() + config.nearOffset, config.near, config.far);
		var clipParent = clipQuad.transform.parent; //camera
		clipQuad.transform.position = clipParent.position + clipParent.forward * dist;

		MonoBehaviour[] behaviours = null;
		bool[] wasEnabled = null;
		if (config.disableStandardAssets)   // 禁用相机上的所有Unity自带标准资源脚本
        {
			behaviours = cam.gameObject.GetComponents<MonoBehaviour>();
			wasEnabled = new bool[behaviours.Length];
			for (int i = 0; i < behaviours.Length; i++)
			{
				var behaviour = behaviours[i];
				if (behaviour.enabled && behaviour.GetType().ToString().StartsWith("UnityStandardAssets.")) //标准脚本的类型都是以UnityStandardAssets开头
                {
					behaviour.enabled = false;
					wasEnabled[i] = true;
				}
			}
		}

		clipQuad.SetActive(true);

        // 相机参数设置后，手动调用render来做一次的渲染（一帧）
		cam.Render();

        // 显示渲染的前景
		Graphics.DrawTexture(new Rect(0, 0, w, h), cam.targetTexture, colorMat);

		var pp = cam.gameObject.GetComponent("PostProcessingBehaviour") as MonoBehaviour;
		if ((pp != null) && pp.enabled)
		{
			pp.enabled = false;
            //cam.Render();
            pp.enabled = true;
		}
        // 显示渲染的alpha
        Graphics.DrawTexture(new Rect(w, 0, w, h), cam.targetTexture, alphaMat);

		clipQuad.SetActive(false);

        // 恢复相机的原始配置
        if (behaviours != null)
		{
			for (int i = 0; i < behaviours.Length; i++)
			{
				if (wasEnabled[i])
				{
					behaviours[i].enabled = true;
				}
			}
		}

		cam.clearFlags = clearFlags;
		cam.backgroundColor = backgroundColor;
	}

    //渲染背景
	public void RenderFar()
	{
		cam.nearClipPlane = config.near;
		cam.farClipPlane = config.far;
		cam.Render();
        //1/4屏幕
		var w = Screen.width / 2;
		var h = Screen.height / 2;

        //渲染到右小角
		Graphics.DrawTexture(new Rect(0, h, w, h), cam.targetTexture, colorMat);
	}

	void OnGUI()
	{
		//Graphics.DrawTexture的必要方法
	}

	Camera[] cameras;
	Rect[] cameraRects;
	float sceneResolutionScale;

    // 激活
	void OnEnable()
	{
        // 将游戏视图的相机移动到右下角的1/4象限
        cameras = FindObjectsOfType<Camera>() as Camera[];  //找到所有的camera
		if (cameras != null)
		{
			var numCameras = cameras.Length;
			cameraRects = new Rect[numCameras];
			for (int i = 0; i < numCameras; i++)
			{
				var cam = cameras[i];
				cameraRects[i] = cam.rect;  //保存所有相机的viewport

                if (cam == this.cam)
					continue;

				if (cam.targetTexture != null)
					continue;

				if (cam.GetComponent<SteamVR_Camera>() != null)
					continue;
                //右小角
				cam.rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
			}
		}
        // 保存场景分辨率缩放因子，后面恢复，默认0.5
        if (config.sceneResolutionScale > 0.0f)
		{
			sceneResolutionScale = SteamVR_Camera.sceneResolutionScale;
			SteamVR_Camera.sceneResolutionScale = config.sceneResolutionScale;
		}
	}

    //关闭
	void OnDisable()
	{
        // 还原游戏视图相机的视口及分辨率缩放因子
        if (cameras != null)
		{
			var numCameras = cameras.Length;
			for (int i = 0; i < numCameras; i++)
			{
				var cam = cameras[i];
				if (cam != null)
					cam.rect = cameraRects[i];
			}
			cameras = null;
			cameraRects = null;
		}

		if (config.sceneResolutionScale > 0.0f)
		{
			SteamVR_Camera.sceneResolutionScale = sceneResolutionScale;
		}
	}
}

