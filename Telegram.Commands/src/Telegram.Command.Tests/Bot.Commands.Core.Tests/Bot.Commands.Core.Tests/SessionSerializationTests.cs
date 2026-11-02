using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class SessionSerializationTests
{
    [Test]
    public void FluentSessionRoundTripPreservesStateAndPayload()
    {
        ISessionInfoWithData session = new CourseSessionDescriptor<FluentObject<Payload, int>>
        {
            Data = new FluentObject<Payload, int>(new Payload { Name = "Ada" }, FireType.HandleCurrent)
            {
                CurrentStateId = new Initiable<int> { Data = 2, IsInit = true },
                ParentStateId = new Initiable<int> { Data = 1, IsInit = true }
            }
        };
        var json = JsonSerializer.Serialize(session.Data);
        var typed = JsonSerializer.Deserialize<FluentObject<Payload, int>>(json);
        var untyped = (FluentObject<Payload, int>)JsonSerializer.Deserialize(json, typeof(FluentObject<Payload, int>));

        foreach (var restored in new[] { typed, untyped })
        {
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored.Object.Name, Is.EqualTo("Ada"));
            Assert.That(restored.CurrentStateId.Data, Is.EqualTo(2));
            Assert.That(restored.CurrentStateId.IsInit, Is.True);
            Assert.That(restored.ParentStateId.Data, Is.EqualTo(1));
            Assert.That(restored.ParentStateId.IsInit, Is.True);
            Assert.That(restored.FireType, Is.EqualTo(FireType.HandleCurrent));
        }
    }

    [Test]
    public void TypedAndUntypedDescriptorsShareData()
    {
        var typed = new CourseSessionDescriptor<Payload> { Data = new Payload { Name = "first" } };
        ISessionInfoWithData untyped = typed;
        Assert.That(untyped.Data, Is.SameAs(typed.Data));
        ((CourseSessionDescriptorBase)typed).Data = new Payload { Name = "updated" };
        Assert.That(typed.Data.Name, Is.EqualTo("updated"));
        typed.Data = null;
        Assert.That(JsonSerializer.Serialize(untyped.Data), Is.EqualTo("null"));
    }

    public class Payload
    {
        public string Name { get; set; }
    }
}
