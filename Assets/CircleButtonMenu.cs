using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// クルクル回るボタンメニュー
/// </summary>
public class CircleButtonMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	[SerializeField] private int _index;
	public int SelectedIndex {
		get { return _index; }
		set {
			_index = value;
			Focus(_index);
		}
	}


	/// <summary>
	/// 半径サイズ
	/// </summary>
	public float Radius {
		get { return _radius; }
		set { _radius = value; }
	}
	[SerializeField] private float _radius = 36f;


	/// <summary>
	/// 回転量を表す角度(0..360)
	/// 値を代入すると自動で位置を更新します.
	/// </summary>
	public float Angle {
		get { return _angle; }
		set {
			_angle = value;
			UpdatePos(_angle);
		}
	}
	[Range(0f, 360f)][SerializeField] private float _angle;




	/// <summary>
	/// 回転するUIオブジェクトのリスト. (read only)
	/// 追加/削除する場合はAddItem/RemoveItemを使用してください.
	/// </summary>
	public List<GameObject> Items {
		get { return _items; }
	}
	[SerializeField] private List<GameObject> _items;



	/// <summary>
	/// 選択中のアイテム
	/// </summary>
	public GameObject SelectedItem {
		get {
			if (_items==null) return null;
			int id = (ItemCount - _index) % ItemCount;
			if (id < 0 || id >= ItemCount) return null;
			return Items[id];
		}
	}

	/// <summary>
	/// アイテム要素数.
	/// </summary>
	public int ItemCount {
		get {
			if (_items == null) return 0;
			return _items.Count;
		}
	}

	/// <summary>
	/// ドラッグ完了してから目標位置へ移動する時のアニメーションの速さ.(0..1)
	/// </summary>
	[Range(0f, 1f)] public float springAmount = 0.3f;

	/// <summary>
	/// 選択中アイテムの拡大率.
	/// </summary>
	[Range(1f, 2f)] public float selectedScaleAmount = 1.2f;

	private float MinDiffAngle = 0.03f;



	/// <summary>
	/// ドラッグ中かどうか.
	/// </summary>
	public bool IsDraging {
		get { return isDraging; }
	}
	private bool isDraging;



	[ContextMenu("Focus")]
	public void Focus() {
		Focus(SelectedIndex);
	}
	/// <summary>
	/// 指定したインデックスのUIが最上部に位置になるようセットします.
	/// </summary>
	public void Focus(int index)
	{
		if (index >= ItemCount) { index %= ItemCount; }
		while (index < 0) { index += ItemCount; }

		_index = index;

		float angle = 0f;
		if (ItemCount != 0)
		{
			angle = 360f / ItemCount * index;
		}
		Angle = angle;

		if (SelectedItem != null && Items != null) {
			foreach (var item in Items) {
				if (item == null) continue;
				if (item == SelectedItem) {
					OnSelect(item);
				} else {
					OnInSelect(item);
				}
			}
		}
	}


	/// <summary>
	/// UIオブジェクトを追加する
	/// </summary>
	public void AddItem(GameObject obj)
	{
		
	}

	/// <summary>
	/// 登録されているUIオブジェクトを削除する
	/// </summary>
	public void RemoveItem(GameObject obj)
	{
		
	}

	/// <summary>
	/// 登録されているUIオブジェクトを削除する.
	/// </summary>
	public void RemoveItem(int index)
	{
		
	}

	void OnSelect(GameObject obj)
	{
		if (obj == null) return;

		obj.transform.localScale = Vector3.one * selectedScaleAmount;
		foreach (var btn in obj.GetComponentsInChildren<Button>(true))
		{
			btn.interactable = true;
		}
	}

	void OnInSelect(GameObject obj)
	{
		if (obj == null) return;

		obj.transform.localScale = Vector3.one;
		foreach (var btn in obj.GetComponentsInChildren<Button>(true))
		{
			btn.interactable = false;
		}
	}


	/// <summary>
	/// 変化角度を元に位置を更新する
	/// </summary>
	private void UpdatePos(float diffAngle)
	{
		GameObject selectedItem = SelectedItem;
		for (int i=0; i<ItemCount; i++)
		{
			if (_items[i] == null) continue;
			if (ItemCount == 0) continue;

			float angle = diffAngle + (360f / ItemCount) * i;
			if (angle < 0) { angle += 360f; }

			GameObject current = _items[i];

			float x = basePos.x + (Mathf.Sin(angle * Mathf.Deg2Rad) * Radius) * transform.localScale.x;
			float y = basePos.y + (Mathf.Cos(angle * Mathf.Deg2Rad) * Radius) * transform.localScale.y;
			float z = current.transform.localPosition.z;
			current.transform.position = new Vector3(x, y, z);

			// Scale更新
			if (_items[i] == selectedItem) {
				OnSelect(_items[i]);
			} else {
				OnInSelect(_items[i]);
			}
		}
	}


	#region Unityイベント ============================================================
	void Awake() {
		
	}

	void Start () {
		UpdatePos(_angle);
	}

	void Update () {
		if (isDraging) return;
		if (ItemCount <= 1) return;

		// 現在角度と目標角度
		float currentAngle = _angle;
		float targetAngle = 360f / ItemCount * _index;
		if (targetAngle >= 360f) targetAngle -= 360f;

		if (currentAngle == targetAngle || targetAngle - currentAngle == 360f) return;// 同じ角度なら何もしない

		float diff = Mathf.Abs(targetAngle - currentAngle);

		if (diff < MinDiffAngle) {
			currentAngle = targetAngle;
		} else {
			if (diff >= 180f) { targetAngle += 360f; }
			currentAngle = targetAngle * springAmount + currentAngle * (1f - springAmount);
		}

		// 角度更新
		Angle = currentAngle;
	}
	#endregion ============================================================


	/// <summary>
	/// 円メニューの中心位置.
	/// </summary>
	private Vector2 basePos { get { return this.transform.position; } }
	/// <summary>
	/// ドラッグの開始位置.
	/// </summary>
	private Vector2 dragStart;
	/// <summary>
	/// ドラッグの終了位置.
	/// </summary>
	private Vector2 dragEnd;

	private float startAndle;
	private float endAngle;

	private float GetAndle(Vector2 p1, Vector2 p2)
	{
		return -Mathf.Atan2(p1.y - p2.y, p1.x - p2.x);
	}

	private float DiffAngleDeg
	{
		get {
			float diffAngle = (endAngle - startAndle) * Mathf.Rad2Deg;
			if (diffAngle >= 180f) { diffAngle -= 360f; }
			if (diffAngle <= -180f) { diffAngle += 360f; }
			return diffAngle;
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		isDraging = true;

		// ドラッグ開始位置
		dragStart = dragEnd = eventData.position;
		startAndle = GetAndle(basePos, dragStart);
		endAngle = GetAndle(basePos, dragEnd);

		prevAngle = endAngle;

//		Debug.Log(string.Format("OnBeginDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg));
	}

	private float prevAngle;
	public void OnDrag (PointerEventData eventData) {
		dragEnd = eventData.position;
		startAndle = GetAndle(basePos, dragStart);
		endAngle = GetAndle(basePos, dragEnd);

		#if true
		// 位置更新
		UpdatePos(Angle + DiffAngleDeg);
		#else
		// 位置更新
		float diff = (_angle + DiffAngleDeg) - prevAngle;
		_angle = _angle + diff;

		prevAngle = _angle + DiffAngleDeg;

		while (_angle > 360f) { _angle -= 360f; }
		while (_angle < 0f) { _angle += 360f; }
		UpdatePos(_angle);

		// もっとも近いindexを計算して設定
		float region = 360f / (ItemCount*2);
		int indexCnt = (int)(_angle / region);
		int nearIndex = indexCnt / 2 + ((indexCnt % 2)==0 ? 0 : 1);
		nearIndex = nearIndex % ItemCount;
		_index = nearIndex;
		#endif

//		Debug.Log(string.Format("OnDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}\n DiffAngle:{5}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg, DiffAngleDeg));
	}

	public void OnEndDrag (PointerEventData eventData) {
		isDraging = false;

		dragEnd = eventData.position;
		startAndle = GetAndle(basePos, dragStart);
		endAngle = GetAndle(basePos, dragEnd);

		// 位置更新
		_angle = _angle + DiffAngleDeg;
		while (_angle > 360f) { _angle -= 360f; }
		while (_angle < 0f) { _angle += 360f; }
		UpdatePos(_angle);

		// もっとも近いindexを計算して設定
		float region = 360f / (ItemCount*2);
		int indexCnt = (int)(_angle / region);
		int nearIndex = indexCnt / 2 + ((indexCnt % 2)==0 ? 0 : 1);
		nearIndex = nearIndex % ItemCount;
		_index = nearIndex;

//		Debug.Log(string.Format("OnEndDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}\n DiffAngle:{5}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg, DiffAngleDeg));
	}
}
