using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Class;

public record struct ImageWrapper(Option<(byte[] Bytes, string Ext)> Option) : IHandlerOutput;