using UnityEngine;
using System.Collections;


public class KalmanFilterFloat
{
	private float _value;
	
	public float Value
	{
		get { return _value; }
		set 
		{ 
			_value = (float) this.update(value); 
		}
	}
	
	private double Q;
	private double R;
	private double P;
	private double X;
	private double K;
	
	public KalmanFilterFloat()
	{
		_value = 0f;
		
		//Q = 0.00001;
		Q = 0.01;
		R = 0.1;
		P = 1;
		X = 0;
	}
	
	private void measurementUpdate()
	{
		K = (P + Q) / (P + Q + R);
		P = R * K;
	}
	
	private double update(double measurement)
	{
		measurementUpdate();
		double result = X + (measurement - X) * K;
		X = result;
		return result;
	}	
}


