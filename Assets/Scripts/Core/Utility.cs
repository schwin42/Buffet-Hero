using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Utility {

	// Convert an object to a byte array
	public static byte[] PayloadToByteArray(Payload obj)
	{
		if(obj == null)
			return null;
		BinaryFormatter bf = new BinaryFormatter();
		MemoryStream ms = new MemoryStream();
		bf.Serialize(ms, obj);
		return ms.ToArray();
	}
	
	// Convert a byte array to an Object
	public static Payload ByteArrayToPayload(byte[] arrBytes)
	{
		MemoryStream memStream = new MemoryStream();
		BinaryFormatter binForm = new BinaryFormatter();
		memStream.Write(arrBytes, 0, arrBytes.Length);
		memStream.Seek(0, SeekOrigin.Begin);
		Payload obj = (Payload) binForm.Deserialize(memStream);
		return obj;
	}
}
