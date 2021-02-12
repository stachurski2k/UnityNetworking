using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public enum ServerPackets{
    Welcome,
    SpawnPlayer,

}
public enum ClientPackets{
    WelcomeEcho,
}
public class Packet:IDisposable{
    List<byte> bytes;
    byte[] serializedBytes;
    int readPos=0;
    public Packet(int _id)
    {
        bytes = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        Write(_id); // Write packet id to the buffer
    }

    public Packet(byte[] _data)
    {
        bytes = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        SetBytes(_data);
    }
    public void SetBytes(byte[] _data)
    {
        Write(_data);
        serializedBytes = bytes.ToArray();
    }
       public void WriteLength()
    {
        bytes.InsertRange(0, BitConverter.GetBytes(bytes.Count)); // Insert the byte length of the packet at the very beginning
    }

    public void InsertInt(int _value)
    {
        bytes.InsertRange(0, BitConverter.GetBytes(_value)); // Insert the int at the start of the buffer
    }

    public byte[] ToArray()
    {
        serializedBytes = bytes.ToArray();
        return serializedBytes;
    }

    public int Length()
    {
        return bytes.Count; // Return the length of buffer
    }

    public int UnreadLength()
    {
        return Length() - readPos; // Return the remaining length (unread)
    }
    #region ReadingData
    public byte ReadByte(){
        if(readPos<bytes.Count){
            var b=serializedBytes[readPos];
            readPos++;
            return b;
        }else {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }
    public byte[] ReadBytes(int length){
        if(readPos<bytes.Count){
            byte[] b=bytes.GetRange(readPos,length).ToArray();
            readPos+=length;
            return b;
        }else {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }
    public byte[] ReadBytes(){
        if(readPos<bytes.Count){
            byte[] b=bytes.GetRange(readPos,bytes.Count-readPos).ToArray();
            readPos+=bytes.Count-readPos;
            return b;
        }else {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }
    public int ReadInt(){
         if(readPos<bytes.Count){
            var b=BitConverter.ToInt32(serializedBytes,readPos);
            readPos+=4;
            return b;
        }else {
            throw new Exception("Could not read value of type 'INT'!");
        }
    }
    public float ReadFloat(){
         if(readPos<bytes.Count){
            var b=BitConverter.ToSingle(serializedBytes,readPos);
            readPos+=4;
            return b;
        }else {
            throw new Exception("Could not read value of type 'float'!");
        }
    }
    public bool ReadBool(){
         if(readPos<bytes.Count){
            var b=BitConverter.ToBoolean(serializedBytes,readPos);
            readPos+=1;
            return b;
        }else {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }
    public string ReadString(){
         if(readPos<bytes.Count){
            int length=ReadInt();
            var str=Encoding.ASCII.GetString(serializedBytes,readPos,length);
            readPos+=length;
            return str;
        }else {
            throw new Exception("Could not read value of type 'strinf'!");
        }
    }
    
    #endregion
    #region WritingData
    public void Write(byte val){
        bytes.Add(val);
    }
    public void Write(byte[] val){
        bytes.AddRange(val);
    }
    public void Write(int val){
        bytes.AddRange(BitConverter.GetBytes(val));
    }
    public void Write(float val){
        bytes.AddRange(BitConverter.GetBytes(val));
    }
    public void Write(bool val){
        bytes.AddRange(BitConverter.GetBytes(val));
    }
    public void Write(string val){
        Write(val.Length);
        bytes.AddRange(Encoding.ASCII.GetBytes(val));
    }
    #endregion
    #region casting
    public static implicit operator byte[](Packet packet){
        return packet.serializedBytes;
    }
    #endregion
    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            bytes.Clear(); // Clear buffer
            serializedBytes = null;
            readPos = 0; // Reset readPos
        }
        else
        {
            readPos -= 4; // "Unread" the last read int
        }
    }
    private bool disposed = false;

    protected virtual void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                bytes = null;
                serializedBytes = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
public class PacketException :Exception{

}