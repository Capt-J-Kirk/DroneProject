using UnityEngine;

namespace Es.InkPainter.Sample
{
	public class MousePainter : MonoBehaviour
	{
		/// <summary>
		/// Types of methods used to paint.
		/// </summary>
		[System.Serializable]
		private enum UseMethodType
		{
			RaycastHitInfo,
			WorldPoint,
			NearestSurfacePoint,
			DirectUV,
		}

		[SerializeField]
		private Brush brush;

		[SerializeField]
		private Camera paintCamera;

		[SerializeField]
		private UseMethodType useMethodType = UseMethodType.RaycastHitInfo;

		[SerializeField]
		bool erase = false;

		private void Update()
		{
			if(Input.GetMouseButton(0))
			{
				//var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var ray = paintCamera.ScreenPointToRay(Input.mousePosition);
                bool success = true;
				RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo))
				{
					Debug.Log("Mouse painter hit with raycast, I hit: " + hitInfo.collider.name);

					Debug.Log("Local point:" + hitInfo.transform.InverseTransformPoint(hitInfo.point));

					var paintObject = hitInfo.transform.GetComponent<InkCanvas>();
					if(paintObject != null)
						switch(useMethodType)
						{
							case UseMethodType.RaycastHitInfo:
								success = erase ? paintObject.Erase(brush, hitInfo) : paintObject.Paint(brush, hitInfo);
								if (success) Debug.Log("I hit: " + hitInfo.point);
								break;

							case UseMethodType.WorldPoint:
								success = erase ? paintObject.Erase(brush, hitInfo.point) : paintObject.Paint(brush, hitInfo.point);
								break;

							case UseMethodType.NearestSurfacePoint:
								success = erase ? paintObject.EraseNearestTriangleSurface(brush, hitInfo.point) : paintObject.PaintNearestTriangleSurface(brush, hitInfo.point);
								break;

							case UseMethodType.DirectUV:
								if(!(hitInfo.collider is MeshCollider))
									Debug.LogWarning("Raycast may be unexpected if you do not use MeshCollider.");
								success = erase ? paintObject.EraseUVDirect(brush, hitInfo.textureCoord) : paintObject.PaintUVDirect(brush, hitInfo.textureCoord);
								break;
						}
					if(!success)
						Debug.LogError("Failed to paint.");
				}
			}
		}

		public void OnGUI()
		{
			if(GUILayout.Button("Reset"))
			{
				foreach(var canvas in FindObjectsOfType<InkCanvas>())
					canvas.ResetPaint();
			}
		}
	}
}