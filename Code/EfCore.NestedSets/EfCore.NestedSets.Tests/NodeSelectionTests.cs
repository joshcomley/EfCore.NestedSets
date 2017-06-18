using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EfCore.NestedSets.Tests
{
    [TestClass]
    public class NodeSelectionTests
    {
        private NodeActionTests _nat;
        NestedSetManager<AppDbContext, Node, int, int?> _ns;

        public NodeSelectionTests()
        {
            _nat = new NodeActionTests();
            _ns = new NestedSetManager<AppDbContext, Node, int, int?>(
                new AppDbContext(), db => db.Nodes);
        }

        [TestInitialize]
        public void SetUp()
        {
            _nat.SetUp();
        }

        [TestMethod]
        public void TestSelectImmediateChildren1()
        {
            // Arrange
            _nat.TestMoveNodeToParentLeft2();
            // Action
            var immediateChildren = _ns.GetImmediateChildren(_nat.Males.Id).ToList();
            // Assert
            AssertResults(immediateChildren, _nat.Josh, _nat.Hairy, _nat.NonHairy);
        }

        [TestMethod]
        public void TestSelectImmediateChildren2()
        {
            // Arrange
            _nat.TestMoveNodeToParentLeft2();
            // Action
            var immediateChildren = _ns.GetImmediateChildren(_nat.Animals.Id).ToList();
            // Assert
            AssertResults(immediateChildren, _nat.Humans, _nat.Dogs, _nat.Pets);
        }

        [TestMethod]
        public void TestSelectImmediateChildren3()
        {
            // Arrange
            _nat.TestInsertChildToLeft2();
            // Action
            var immediateChildren = _ns.GetImmediateChildren(_nat.Cats.Id).ToList();
            // Assert
            AssertResults(immediateChildren, _nat.HouseCats, _nat.Tigers);
        }

        [TestMethod]
        public void TestSelectDescendants1()
        {
            // Arrange
            _nat.TestMoveNodeToParentLeft2();
            // Action
            var result = _ns.GetDescendants(_nat.Males.Id).ToList();
            // Assert
            AssertResults(result, _nat.Josh, _nat.Hairy, _nat.NonHairy);
        }

        [TestMethod]
        public void TestSelectDescendants2()
        {
            // Arrange
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetDescendants(_nat.Pets.Id).ToList();
            // Assert
            AssertResults(result,
                _nat.Humans,
                _nat.Males,
                _nat.Hairy,
                _nat.NonHairy,
                _nat.Josh,
                _nat.Females,
                _nat.Cats,
                _nat.HouseCats,
                _nat.Kittens,
                _nat.Tigers);
        }

        [TestMethod]
        public void TestSelectDescendantsWithMultipleTrees()
        {
            // Arrange
            _nat.TestInsertSecondLevelChild();
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetDescendants(_nat.Pets.Id).ToList();
            // Assert
            AssertResults(result,
                _nat.Humans,
                _nat.Males,
                _nat.Hairy,
                _nat.NonHairy,
                _nat.Josh,
                _nat.Females,
                _nat.Cats,
                _nat.HouseCats,
                _nat.Kittens,
                _nat.Tigers);
        }

        [TestMethod]
        public void TestSelectSpecificDepthOfDescendants1()
        {
            // Arrange
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetDescendants(_nat.Pets.Id, 1).ToList();
            // Assert
            AssertResults(result,
                _nat.Humans,
                _nat.Cats);
        }

        [TestMethod]
        public void TestSelectSpecificDepthOfDescendants2()
        {
            // Arrange
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetDescendants(_nat.Animals.Id, 2).ToList();
            // Assert
            AssertResults(result,
                _nat.Dogs,
                _nat.Pets,
                _nat.Humans,
                _nat.Cats);
        }

        [TestMethod]
        public void TestSelectPathToNode1()
        {
            // Arrange
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetPathToNode(_nat.HouseCats.Id).ToList();
            // Assert
            AssertResultsAndOrder(result,
                _nat.Animals,
                _nat.Pets,
                _nat.Cats
            );
        }

        [TestMethod]
        public void TestSelectPathToNode2()
        {
            // Arrange
            _nat.TestMoveToSiblingRight();
            // Action
            var result = _ns.GetPathToNode(_nat.Humans.Id).ToList();
            // Assert
            AssertResultsAndOrder(result,
                _nat.Animals,
                _nat.Pets
            );
        }

        [TestMethod]
        public void TestSelectPathToNode3()
        {
            // Arrange
            _nat.TestInsertChildToRight();
            // Action
            var result = _ns.GetPathToNode(_nat.Dogs.Id).ToList();
            // Assert
            AssertResultsAndOrder(result,
                _nat.Animals
            );
        }

        // TODO:
        // Test selecting X levels deep
        // Test 
        private static void AssertResultsAndOrder(IEnumerable<Node> nodesQuery, params Node[] expectedNodes)
        {
            AssertResults(nodesQuery, true, expectedNodes);
        }

        private static void AssertResults(IEnumerable<Node> nodesQuery, params Node[] expectedNodes)
        {
            AssertResults(nodesQuery, false, expectedNodes);
        }

        private static void AssertResults(IEnumerable<Node> nodesQuery, bool checkOrder, params Node[] expectedNodes)
        {
            var actualNodes = nodesQuery.ToList();
            Assert.AreEqual(actualNodes.Count, actualNodes.Count);
            var index = 0;
            foreach (var expectedNode in expectedNodes)
            {
                var node = actualNodes.SingleOrDefault(n => n.Name == expectedNode.Name);
                Assert.IsNotNull(node);
                if (checkOrder)
                {
                    Assert.AreEqual(index, actualNodes.IndexOf(node));
                }
                index++;
            }
        }
    }
}