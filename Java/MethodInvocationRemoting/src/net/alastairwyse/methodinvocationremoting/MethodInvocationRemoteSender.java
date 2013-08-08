/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package net.alastairwyse.methodinvocationremoting;

/**
 * Sends method invocations (represented by IMethodInvocation objects) to remote locations.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteSender implements IMethodInvocationRemoteSender {

    private IMethodInvocationSerializer serializer;
    private IRemoteSender sender;
    private IRemoteReceiver receiver;
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteSender class.
     * @param serializer  Object to use to serialize method invocations.
     * @param sender      Object to use to send serialized method invocations.
     * @param receiver    Object to use to send serialized method invocations.
     */
    public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver)
    {
        this.serializer = serializer;
        this.sender = sender;
        this.receiver = receiver;
    }
    
    @Override
    public Object InvokeMethod(IMethodInvocation inputMethodInvocation) throws Exception {
        Object returnValue;

        // Check that inputted method invocation does not have a void return type.
        if (inputMethodInvocation.getReturnType() == null)
        {
            throw new IllegalArgumentException("Method invocation cannot have a void return type.");
        }

        String serializedReturnValue = SerializeAndSend(inputMethodInvocation);
        try
        {
            returnValue = serializer.DeserializeReturnValue(serializedReturnValue);
        }
        catch (Exception e)
        {
            throw new DeserializationException("Failed to deserialize return value.", e);
        }
        
        return returnValue;
    }

    @Override
    public void InvokeVoidMethod(IMethodInvocation inputMethodInvocation) throws Exception {
        // Check that inputted method invocation has a void return type.
        if (inputMethodInvocation.getReturnType() != null)
        {
            throw new IllegalArgumentException("Method invocation must have a void return type.");
        }

        String serializedReturnValue = SerializeAndSend(inputMethodInvocation);
        if (serializedReturnValue.equals(serializer.getVoidReturnValue()) == false)
        {
            throw new Exception("Invocation of void method returned non-void.");
        }
    }
    
    /**
     * Provides common method invocation serialization and sending functionality to public methods.
     * @param inputMethodInvocation  The method invocation to serialize and send.
     * @return                       The serialized return value of the method invocation.
     * @throws Exception
     */
    private String SerializeAndSend(IMethodInvocation inputMethodInvocation) throws Exception
    {
        try
        {
            String serializedMethodInvocation = serializer.Serialize(inputMethodInvocation);
            sender.Send(serializedMethodInvocation);
            return receiver.Receive();
        }
        catch (Exception e)
        {
            throw new Exception("Failed to invoke method.", e);
        }
    }

}