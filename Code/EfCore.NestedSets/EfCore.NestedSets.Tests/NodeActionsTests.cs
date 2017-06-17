using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCore.NestedSets.Tests
{
    [TestClass]
    public class NodeActionsTests
    {
        [TestInitialize]
        public void SetUp()
        {
            DbSql.CreateDatabase();
            new AppDbContext().Database.Migrate();
        }

        [TestCleanup]
        public void TearDown()
        {
            //DbSql.RunDbSql("DELETE FROM Nodes");
        }

        private static Node NewNode(string name)
        {
            return new Node { Name = name };
        }

        [TestMethod]
        public void TestMultipleHierarchies()
        {
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
        }

        public void TestAnimalsHierarchy()
        {
            Node animals, humans, males, females, cats, houseCats, tigers, dogs, kittens, hairy, nonHairy, pets, josh;
            using (var db = new AppDbContext())
            {
                var ns = new NestedSetManager<Node, int, int?>(db, db.Nodes);

                animals = ns.InsertRoot(NewNode("Animals"), NestedSetInsertMode.Right);
                AssertDb(animals.RootId, new Node(animals.Name, null, 0, 1, 2));

                humans = ns.InsertBelow(animals.Id, NewNode("Humans"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 4),
                    new Node(humans.Name, animals.Id, 1, 2, 3)
                );

                males = ns.InsertBelow(humans.Id, NewNode("Males"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 6),
                    new Node(humans.Name, animals.Id, 1, 2, 5),
                    new Node(males.Name, humans.Id, 2, 3, 4)
                );

                females = ns.InsertNextTo(males.Id, NewNode("Females"), NestedSetInsertMode.Right);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 8),
                    new Node(humans.Name, animals.Id, 1, 2, 7),
                    new Node(males.Name, humans.Id, 2, 3, 4),
                    new Node(females.Name, humans.Id, 2, 5, 6)
                );

                cats = ns.InsertNextTo(humans.Id, NewNode("Cats"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 10),
                    new Node(cats.Name, animals.Id, 1, 2, 3),
                    new Node(humans.Name, animals.Id, 1, 4, 9),
                    new Node(males.Name, humans.Id, 2, 5, 6),
                    new Node(females.Name, humans.Id, 2, 7, 8)
                );

                houseCats = ns.InsertBelow(cats.Id, NewNode("House cats"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 12),
                    new Node(cats.Name, animals.Id, 1, 2, 5),
                    new Node(houseCats.Name, cats.Id, 2, 3, 4),
                    new Node(humans.Name, animals.Id, 1, 6, 11),
                    new Node(males.Name, humans.Id, 2, 7, 8),
                    new Node(females.Name, humans.Id, 2, 9, 10)
                );

                tigers = ns.InsertNextTo(houseCats.Id, NewNode("Tigers"), NestedSetInsertMode.Right);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 14),
                    new Node(cats.Name, animals.Id, 1, 2, 7),
                    new Node(houseCats.Name, cats.Id, 2, 3, 4),
                    new Node(tigers.Name, cats.Id, 2, 5, 6),
                    new Node(humans.Name, animals.Id, 1, 8, 13),
                    new Node(males.Name, humans.Id, 2, 9, 10),
                    new Node(females.Name, humans.Id, 2, 11, 12)
                );

                dogs = ns.InsertBelow(animals.Id, NewNode("Dogs"), NestedSetInsertMode.Right);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 16),
                    new Node(cats.Name, animals.Id, 1, 2, 7),
                    new Node(houseCats.Name, cats.Id, 2, 3, 4),
                    new Node(tigers.Name, cats.Id, 2, 5, 6),
                    new Node(humans.Name, animals.Id, 1, 8, 13),
                    new Node(males.Name, humans.Id, 2, 9, 10),
                    new Node(females.Name, humans.Id, 2, 11, 12),
                    new Node(dogs.Name, animals.Id, 1, 14, 15)
                );

                kittens = ns.InsertBelow(houseCats.Id, NewNode("Kittens"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 18),
                    new Node(cats.Name, animals.Id, 1, 2, 9),
                    new Node(houseCats.Name, cats.Id, 2, 3, 6),
                    new Node(kittens.Name, houseCats.Id, 3, 4, 5),
                    new Node(tigers.Name, cats.Id, 2, 7, 8),
                    new Node(humans.Name, animals.Id, 1, 10, 15),
                    new Node(males.Name, humans.Id, 2, 11, 12),
                    new Node(females.Name, humans.Id, 2, 13, 14),
                    new Node(dogs.Name, animals.Id, 1, 16, 17)
                );

                hairy = ns.InsertBelow(males.Id, NewNode("Hairy"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 20),
                    new Node(cats.Name, animals.Id, 1, 2, 9),
                    new Node(houseCats.Name, cats.Id, 2, 3, 6),
                    new Node(kittens.Name, houseCats.Id, 3, 4, 5),
                    new Node(tigers.Name, cats.Id, 2, 7, 8),
                    new Node(humans.Name, animals.Id, 1, 10, 17),
                    new Node(males.Name, humans.Id, 2, 11, 14),
                    new Node(hairy.Name, males.Id, 3, 12, 13),
                    new Node(females.Name, humans.Id, 2, 15, 16),
                    new Node(dogs.Name, animals.Id, 1, 18, 19)
                );

                nonHairy = ns.InsertBelow(males.Id, NewNode("Non-Hairy"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 22),
                    new Node(cats.Name, animals.Id, 1, 2, 9),
                    new Node(houseCats.Name, cats.Id, 2, 3, 6),
                    new Node(kittens.Name, houseCats.Id, 3, 4, 5),
                    new Node(tigers.Name, cats.Id, 2, 7, 8),
                    new Node(humans.Name, animals.Id, 1, 10, 19),
                    new Node(males.Name, humans.Id, 2, 11, 16),
                    new Node(nonHairy.Name, males.Id, 3, 12, 13),
                    new Node(hairy.Name, males.Id, 3, 14, 15),
                    new Node(females.Name, humans.Id, 2, 17, 18),
                    new Node(dogs.Name, animals.Id, 1, 20, 21)
                );

                josh = ns.InsertBelow(males.Id, NewNode("Josh"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 24),
                    new Node(cats.Name, animals.Id, 1, 2, 9),
                    new Node(houseCats.Name, cats.Id, 2, 3, 6),
                    new Node(kittens.Name, houseCats.Id, 3, 4, 5),
                    new Node(tigers.Name, cats.Id, 2, 7, 8),
                    new Node(humans.Name, animals.Id, 1, 10, 21),
                    new Node(males.Name, humans.Id, 2, 11, 18),
                    new Node(josh.Name, males.Id, 3, 12, 13),
                    new Node(nonHairy.Name, males.Id, 3, 14, 15),
                    new Node(hairy.Name, males.Id, 3, 16, 17),
                    new Node(females.Name, humans.Id, 2, 19, 20),
                    new Node(dogs.Name, animals.Id, 1, 22, 23)
                );

                // Now let's delete Cats
                var catsTree = ns.Delete(cats.Id);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 16),
                    new Node(humans.Name, animals.Id, 1, 2, 13),
                    new Node(males.Name, humans.Id, 2, 3, 10),
                    new Node(josh.Name, males.Id, 3, 4, 5),
                    new Node(nonHairy.Name, males.Id, 3, 6, 7),
                    new Node(hairy.Name, males.Id, 3, 8, 9),
                    new Node(females.Name, humans.Id, 2, 11, 12),
                    new Node(dogs.Name, animals.Id, 1, 14, 15)
                );

                // Create the pets node
                pets = ns.InsertBelow(animals.Id, NewNode("Pets"), NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 18),
                    new Node(pets.Name, animals.Id, 1, 2, 3),
                    new Node(humans.Name, animals.Id, 1, 4, 15),
                    new Node(males.Name, humans.Id, 2, 5, 12),
                    new Node(josh.Name, males.Id, 3, 6, 7),
                    new Node(nonHairy.Name, males.Id, 3, 8, 9),
                    new Node(hairy.Name, males.Id, 3, 10, 11),
                    new Node(females.Name, humans.Id, 2, 13, 14),
                    new Node(dogs.Name, animals.Id, 1, 16, 17)
                );

                // And insert the removed Cats node under Pets
                catsTree = ns.InsertBelow(pets.Id, catsTree, NestedSetInsertMode.Right);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 26),
                    new Node(pets.Name, animals.Id, 1, 2, 11),
                    new Node(cats.Name, pets.Id, 2, 3, 10),
                    new Node(houseCats.Name, cats.Id, 3, 4, 7),
                    new Node(kittens.Name, houseCats.Id, 4, 5, 6),
                    new Node(tigers.Name, cats.Id, 3, 8, 9),
                    new Node(humans.Name, animals.Id, 1, 12, 23),
                    new Node(males.Name, humans.Id, 2, 13, 20),
                    new Node(josh.Name, males.Id, 3, 14, 15),
                    new Node(nonHairy.Name, males.Id, 3, 16, 17),
                    new Node(hairy.Name, males.Id, 3, 18, 19),
                    new Node(females.Name, humans.Id, 2, 21, 22),
                    new Node(dogs.Name, animals.Id, 1, 24, 25)
                );

                ns.MoveToParent(humans.Id, pets.Id, NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 26),
                    new Node(pets.Name, animals.Id, 1, 2, 23),
                    new Node(humans.Name, pets.Id, 2, 3, 14),
                    new Node(males.Name, humans.Id, 3, 4, 11),
                    new Node(josh.Name, males.Id, 4, 5, 6),
                    new Node(nonHairy.Name, males.Id, 4, 7, 8),
                    new Node(hairy.Name, males.Id, 4, 9, 10),
                    new Node(females.Name, humans.Id, 3, 12, 13),
                    new Node(cats.Name, pets.Id, 2, 15, 22),
                    new Node(houseCats.Name, cats.Id, 3, 16, 19),
                    new Node(kittens.Name, houseCats.Id, 4, 17, 18),
                    new Node(tigers.Name, cats.Id, 3, 20, 21),
                    new Node(dogs.Name, animals.Id, 1, 24, 25)
                );

                ns.MoveToSibling(pets.Id, dogs.Id, NestedSetInsertMode.Right);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 26),
                    new Node(dogs.Name, animals.Id, 1, 2, 3),
                    new Node(pets.Name, animals.Id, 1, 4, 25),
                    new Node(humans.Name, pets.Id, 2, 5, 16),
                    new Node(males.Name, humans.Id, 3, 6, 13),
                    new Node(josh.Name, males.Id, 4, 7, 8),
                    new Node(nonHairy.Name, males.Id, 4, 9, 10),
                    new Node(hairy.Name, males.Id, 4, 11, 12),
                    new Node(females.Name, humans.Id, 3, 14, 15),
                    new Node(cats.Name, pets.Id, 2, 17, 24),
                    new Node(houseCats.Name, cats.Id, 3, 18, 21),
                    new Node(kittens.Name, houseCats.Id, 4, 19, 20),
                    new Node(tigers.Name, cats.Id, 3, 22, 23)
                );

                ns.MoveToParent(humans.Id, animals.Id, NestedSetInsertMode.Left);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 26),
                    new Node(humans.Name, animals.Id, 1, 2, 13),
                    new Node(males.Name, humans.Id, 2, 3, 10),
                    new Node(josh.Name, males.Id, 3, 4, 5),
                    new Node(nonHairy.Name, males.Id, 3, 6, 7),
                    new Node(hairy.Name, males.Id, 3, 8, 9),
                    new Node(females.Name, humans.Id, 2, 11, 12),
                    new Node(dogs.Name, animals.Id, 1, 14, 15),
                    new Node(pets.Name, animals.Id, 1, 16, 25),
                    new Node(cats.Name, pets.Id, 2, 17, 24),
                    new Node(houseCats.Name, cats.Id, 3, 18, 21),
                    new Node(kittens.Name, houseCats.Id, 4, 19, 20),
                    new Node(tigers.Name, cats.Id, 3, 22, 23)
                );

                ns.Delete(pets.Id);
                AssertDb(
                    animals.RootId,
                    new Node(animals.Name, null, 0, 1, 16),
                    new Node(humans.Name, animals.Id, 1, 2, 13),
                    new Node(males.Name, humans.Id, 2, 3, 10),
                    new Node(josh.Name, males.Id, 3, 4, 5),
                    new Node(nonHairy.Name, males.Id, 3, 6, 7),
                    new Node(hairy.Name, males.Id, 3, 8, 9),
                    new Node(females.Name, humans.Id, 2, 11, 12),
                    new Node(dogs.Name, animals.Id, 1, 14, 15)
                );
            }
        }

        private static void AssertDb(int? rootId, params Node[] expectedNodes)
        {
            using (var db = new AppDbContext())
            {
                var nodes = db.Nodes.Where(n => n.RootId == rootId);
                Assert.AreEqual(expectedNodes.Length, nodes.Count());
                for (var i = 0; i < expectedNodes.Length; i++)
                {
                    var node = nodes.SingleOrDefault(n => n.Name == expectedNodes[i].Name);
                    Assert.AreEqual(rootId, node.RootId);
                    Assert.AreEqual(expectedNodes[i].Left, node.Left);
                    Assert.AreEqual(expectedNodes[i].Right, node.Right);
                    Assert.AreEqual(expectedNodes[i].ParentId, node.ParentId);
                    Assert.AreEqual(expectedNodes[i].Level, node.Level);
                }
            }
        }
    }
}
