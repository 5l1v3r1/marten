using System;
using System.Linq;
using Marten.Events.Projections;
using Marten.Services;
using Shouldly;
using Xunit;

namespace Marten.Testing.Events.Projections
{
    public class view_projection_for_view_with_base_class: DocumentSessionFixture<NulloIdentityMap>
    {
        private readonly MonsterSlayed slayed1 = new MonsterSlayed { Name = "Troll" };
        private readonly MonsterSlayed slayed2 = new MonsterSlayed { Name = "Dragon" };
        private readonly Guid streamId;

        public view_projection_for_view_with_base_class()
        {
            StoreOptions(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Events.InlineProjections.Add<ViewProjectionForViewWithBaseClass>();
                _.Events.InlineProjections.Add<ViewProjectionForViewWithBaseClassAndIdOverloaded>();
                _.Events.InlineProjections.Add<ViewProjectionForViewWithBaseClassAndIdOverloadedWithNew>();
            });

            streamId = theSession.Events
                .StartStream<QuestMonstersWithBaseClass>(slayed1, slayed2).Id;

            theSession.SaveChanges();
        }

        [Fact]
        public void run_view_projection_with_base_class()
        {
            VerifyProjection<QuestMonstersWithBaseClass>();
        }

        [Fact]
        public void run_view_projection_with_base_class_and_id_overloaded()
        {
            VerifyProjection<QuestMonstersWithBaseClassAndIdOverloaded>();
        }

        [Fact]
        public void run_view_projection_with_base_class_and_id_overloaded_with_new()
        {
            VerifyProjection<QuestMonstersWithBaseClassAndIdOverloadedWithNew>();
        }

        private void VerifyProjection<T>() where T : IMonstersView
        {
            var loadedView = theSession.Load<T>(streamId);

            loadedView.Id.ShouldBe(streamId);
            loadedView.Monsters.ShouldHaveTheSameElementsAs("Troll", "Dragon");

            var queriedView = theSession.Query<T>()
                .Single(x => x.Id == streamId);

            queriedView.Id.ShouldBe(streamId);
            queriedView.Monsters.ShouldHaveTheSameElementsAs("Troll", "Dragon");
        }

        public class ViewProjectionForViewWithBaseClass: ViewProjection<QuestMonstersWithBaseClass, Guid>
        {
            public ViewProjectionForViewWithBaseClass()
            {
                ProjectEvent<MonsterSlayed>(Project);
            }

            private void Project(QuestMonstersWithBaseClass view, MonsterSlayed @event)
            {
                view.Apply(@event);
            }
        }

        public class ViewProjectionForViewWithBaseClassAndIdOverloaded: ViewProjection<QuestMonstersWithBaseClassAndIdOverloaded, Guid>
        {
            public ViewProjectionForViewWithBaseClassAndIdOverloaded()
            {
                ProjectEvent<MonsterSlayed>(Project);
            }

            private void Project(QuestMonstersWithBaseClassAndIdOverloaded view, MonsterSlayed @event)
            {
                view.Apply(@event);
            }
        }

        public class ViewProjectionForViewWithBaseClassAndIdOverloadedWithNew: ViewProjection<QuestMonstersWithBaseClassAndIdOverloadedWithNew, Guid>
        {
            public ViewProjectionForViewWithBaseClassAndIdOverloadedWithNew()
            {
                ProjectEvent<MonsterSlayed>(Project);
            }

            private void Project(QuestMonstersWithBaseClassAndIdOverloadedWithNew view, MonsterSlayed @event)
            {
                view.Apply(@event);
            }
        }
    }
}
