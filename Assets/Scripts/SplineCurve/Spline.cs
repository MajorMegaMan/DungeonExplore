using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adapted from OneLoneCoder
// https://github.com/OneLoneCoder/videos/blob/master/OneLoneCoder_Splines1.cpp

[System.Serializable]
public class Spline
{
	[SerializeField] List<Vector3> m_points;

	public List<Vector3> points { get { return m_points; } }

	public Spline()
    {
		m_points = new List<Vector3>();
    }

	public Vector3 GetSplinePoint(float t, bool looped = false)
    {
		int p0, p1, p2, p3;
		if (!looped)
		{
			p1 = (int)t + 1;
			p2 = p1 + 1;
			p3 = p2 + 1;
			p0 = p1 - 1;
		}
		else
		{
			p1 = (int)t;
			p2 = (p1 + 1) % m_points.Count;
			p3 = (p2 + 1) % m_points.Count;
			p0 = p1 >= 1 ? p1 - 1 : m_points.Count - 1;
		}

		t = t - (int)t;

		float tt = t * t;
		float ttt = tt * t;

		float q1 = -ttt + 2.0f * tt - t;
		float q2 = 3.0f * ttt - 5.0f * tt + 2.0f;
		float q3 = -3.0f * ttt + 4.0f * tt + t;
		float q4 = ttt - tt;

		Vector3 result = Vector3.zero;

		float tx = 0.5f * (m_points[p0].x * q1 + m_points[p1].x * q2 + m_points[p2].x * q3 + m_points[p3].x * q4);
		float ty = 0.5f * (m_points[p0].y * q1 + m_points[p1].y * q2 + m_points[p2].y * q3 + m_points[p3].y * q4);
		float tz = 0.5f * (m_points[p0].z * q1 + m_points[p1].z * q2 + m_points[p2].z * q3 + m_points[p3].z * q4);

		result.x = tx;
		result.y = ty;
		result.z = tz;

		return result;
	}

	public Vector3 GetSplineGradient(float t, bool looped = false)
	{
		int p0, p1, p2, p3;
		if (!looped)
		{
			p1 = (int)t + 1;
			p2 = p1 + 1;
			p3 = p2 + 1;
			p0 = p1 - 1;
		}
		else
		{
			p1 = (int)t;
			p2 = (p1 + 1) % m_points.Count;
			p3 = (p2 + 1) % m_points.Count;
			p0 = p1 >= 1 ? p1 - 1 : m_points.Count - 1;
		}

		t = t - (int)t;

		float tt = t * t;
		float ttt = tt * t;

		float q1 = -3.0f * tt + 4.0f * t - 1;
		float q2 = 9.0f * tt - 10.0f * t;
		float q3 = -9.0f * tt + 8.0f * t + 1.0f;
		float q4 = 3.0f * tt - 2.0f * t;

		Vector3 result = Vector3.zero;

		float tx = 0.5f * (m_points[p0].x * q1 + m_points[p1].x * q2 + m_points[p2].x * q3 + m_points[p3].x * q4);
		float ty = 0.5f * (m_points[p0].y * q1 + m_points[p1].y * q2 + m_points[p2].y * q3 + m_points[p3].y * q4);
		float tz = 0.5f * (m_points[p0].z * q1 + m_points[p1].z * q2 + m_points[p2].z * q3 + m_points[p3].z * q4);

		result.x = tx;
		result.y = ty;
		result.z = tz;

		return result;
	}
}

[System.Serializable]
public class CustomSpline<T> where T : ISplinePoint
{
	[SerializeField] List<T> m_points;

	public List<T> points { get { return m_points; } }

	public CustomSpline()
	{
		m_points = new List<T>();
	}

	public Vector3 GetSplinePoint(float t, bool looped = false)
	{
		int p0, p1, p2, p3;
		if (!looped)
		{
			if((int)t >= m_points.Count - 3)
            {
				return m_points[m_points.Count - 2].GetPoint();
			}

			p1 = (int)t + 1;
			p2 = p1 + 1;
			p3 = p2 + 1;
			p0 = p1 - 1;
		}
		else
		{
			p1 = (int)t % m_points.Count;
			p2 = (p1 + 1) % m_points.Count;
			p3 = (p2 + 1) % m_points.Count;
			p0 = p1 >= 1 ? p1 - 1 : m_points.Count - 1;
		}

		t = t - (int)t;

		float tt = t * t;
		float ttt = tt * t;

		float q1 = -ttt + 2.0f * tt - t;
		float q2 = 3.0f * ttt - 5.0f * tt + 2.0f;
		float q3 = -3.0f * ttt + 4.0f * tt + t;
		float q4 = ttt - tt;

		Vector3 result = Vector3.zero;

		float tx = 0.5f * (m_points[p0].x * q1 + m_points[p1].x * q2 + m_points[p2].x * q3 + m_points[p3].x * q4);
		float ty = 0.5f * (m_points[p0].y * q1 + m_points[p1].y * q2 + m_points[p2].y * q3 + m_points[p3].y * q4);
		float tz = 0.5f * (m_points[p0].z * q1 + m_points[p1].z * q2 + m_points[p2].z * q3 + m_points[p3].z * q4);

		result.x = tx;
		result.y = ty;
		result.z = tz;

		return result;
	}

	public Vector3 GetSplineGradient(float t, bool looped = false)
	{
		int p0, p1, p2, p3;
		if (!looped)
		{
			if ((int)t >= m_points.Count - 3)
			{
				// change t to be a very small error before the last index, to prevent an out of range exception from occuring.
				t = (int)t;
				t -= 0.0001f;
			}

			p1 = (int)t + 1;
			p2 = p1 + 1;
			p3 = p2 + 1;
			p0 = p1 - 1;
		}
		else
		{
			p1 = (int)t % m_points.Count;
			p2 = (p1 + 1) % m_points.Count;
			p3 = (p2 + 1) % m_points.Count;
			p0 = p1 >= 1 ? p1 - 1 : m_points.Count - 1;
		}

		t = t - (int)t;

		float tt = t * t;
		float ttt = tt * t;

		float q1 = -3.0f * tt + 4.0f * t - 1;
		float q2 = 9.0f * tt - 10.0f * t;
		float q3 = -9.0f * tt + 8.0f * t + 1.0f;
		float q4 = 3.0f * tt - 2.0f * t;

		Vector3 result = Vector3.zero;

		float tx = 0.5f * (m_points[p0].x * q1 + m_points[p1].x * q2 + m_points[p2].x * q3 + m_points[p3].x * q4);
		float ty = 0.5f * (m_points[p0].y * q1 + m_points[p1].y * q2 + m_points[p2].y * q3 + m_points[p3].y * q4);
		float tz = 0.5f * (m_points[p0].z * q1 + m_points[p1].z * q2 + m_points[p2].z * q3 + m_points[p3].z * q4);

		result.x = tx;
		result.y = ty;
		result.z = tz;

		return result;
	}
}

public interface ISplinePoint
{
	float x { get { return GetPoint().x; } }
	float y { get { return GetPoint().y; } }
	float z { get { return GetPoint().z; } }
	Vector3 GetPoint();
}
