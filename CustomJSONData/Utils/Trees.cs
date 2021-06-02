﻿using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using ArrayList = System.Collections.ArrayList;

namespace CustomJSONData
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TreeDict = JObject;
    using TreeType = JObject;

    /// <summary>
    /// Utility functions for creating and manipulating Trees, which are <see cref="Dictionary{string, object}" /> guaranteed to have the following properties:
    /// - Implements all the features of the Dictionary class that would be visible to standard C# code (notably including implementation of<see cref="IDictionary{string, object}"/>).
    /// - No members of a Tree will be Trees with the original Tree as a member.
    /// - All Trees will be of the same underlying type; however, the actual type used is an implementation detail and should not be relied upon.
    ///
    /// Note that calling many of the functions in this class on invalid Trees (those that contain themselves directly or indirectly) is undefined behavior, and may lead to infinite loops
    /// or stack overflows. To avoid inadvertently creating invalid Trees, we recommend never adding a subtree to a Tree that's not newly created, although validating defensively with
    /// <see cref="isTree(object)"/> is also an option.
    /// </summary>
    public static class Trees
    {
        /// <summary>
        /// Used to construct a new Tree, as it does not exist as a proper class (by design).
        /// </summary>
        /// <returns>An empty Tree.</returns>
        public static TreeDict Tree() => new JObject();

        /// <summary>
        /// Used to more easily construct a Tree with an initial set of members.
        /// </summary>
        /// <param name="contents">The members to initialize the new Tree with.</param>
        /// <returns>A new Tree containing the provided members.</returns>
        public static JObject Tree(IEnumerable<KeyValuePair<string, object>> contents)
        {
            TreeDict t = Tree();
            foreach (KeyValuePair<string, object> p in contents)
            {
                t[p.Key] = JToken.FromObject(p.Value);
            }

            return t;
        }

        /// <summary>
        /// Used to more easily construct a Tree with an initial set of members.
        /// </summary>
        /// <param name="contents">The members to initialize the new Tree with.</param>
        /// <returns>A new Tree containing the provided members.</returns>
        public static JObject Tree(TreeDict contents)
        {
            TreeDict t = Tree();
            foreach (KeyValuePair<string, JToken> p in contents)
            {
                t[p.Key] = p.Value;
            }

            return t;
        }

        public static TreeDict fromJSON(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            return JsonConvert.DeserializeObject<TreeDict>(json, settings);
        }

        /// <summary>
        /// Makes a semi-deep copy of a tree - members of non-Tree reference types in copy(<paramref name="t"/>) will refer to the same objects as in <paramref name="t"/>, but Tree members will be recursively copied.
        /// </summary>
        /// <param name="t">The tree to copy.</param>
        /// <returns>A copy of t.</returns>
        public static JObject copy(TreeDict t)
        {
            TreeDict result = Tree();
            foreach (KeyValuePair<string, JToken> p in t)
            {
                result[p.Key] = JToken.FromObject(p.Value is TreeType ? copy(p.Value as TreeType) : p.Value);
            }

            return result;
        }

        /// <summary>
        /// The extension method form of <see cref="copy(TreeType)"/>
        /// </summary>
        /// <seealso cref="copy(TreeType)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeDict Copy(this TreeType t) => copy(t);

        /// <summary>
        /// Makes a shallow copy of a tree - all members of reference types, including sub-Trees, will refer to the same objects as in <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The tree to copy.</param>
        /// <returns>A copy of <paramref name="t"/>.</returns>
        public static TreeDict shallowCopy(TreeType t) => Tree(t);

        /// <summary>
        /// The extension method form of <see cref="shallowCopy(TreeType)"/>
        /// </summary>
        /// <seealso cref="shallowCopy(TreeType)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeDict ShallowCopy(this TreeType t) => shallowCopy(t);

        /// <summary>
        /// Safely accesses a member of a tree, returning null if the member does not exist or if <paramref name="tree"/> is null.
        /// </summary>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or null if tree is null or no such member exists</returns>
        public static JToken at(TreeDict tree, string memberName)
        {
            if (tree == null) return null;
            tree.TryGetValue(memberName, out JToken result);
            return result;
        }

        // TODO: This is probably bad, serializing only to deserialize. This is gonna need fixing either with more breaking changes to mods or just avoiding it.
        /// <summary>
        /// Safely accesses a member of a tree, returning null if the member does not exist or if <paramref name="tree"/> is null.
        /// </summary>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or null if tree is null or no such member exists</returns>
        [CanBeNull]
        public static T at<T>(TreeDict tree, string memberName)
        {
            if (tree == null)
                return default;

            tree.TryGetValue(memberName, out JToken result);

            switch (result)
            {
                case T rT:
                    return rT;
                case JToken rs:
                    return rs.ToObject<T>();
                default:
                    return default;
            }
        }

        /// <summary>
        /// Safely accesses a member of a tree, returning null if the member does not exist or if <paramref name="tree"/> is null.
        /// </summary>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or null if tree is null or no such member exists</returns>
        public static TreeDict at(Dictionary<string, TreeDict> tree, string memberName)
        {
            if (tree == null) return null;
            tree.TryGetValue(memberName, out TreeDict result);
            return result;
        }

        /// <summary>
        /// The extension method form of <see cref="at(TreeDict, string)"/>
        /// </summary>
        /// <seealso cref="at(TreeDict, string)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object At(this TreeDict t, string m) => at(t, m);

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// If <typeparamref name="T"/> is a value type and you want <paramref name="defaultValue"/> to be null, use <see cref="getNullable{T}(TreeDict, string, T?)"/>.
        /// If you want <typeparamref name="T"/> to be Tree, use <see cref="getTree(TreeDict, string, object)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the member to be returned</typeparam>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        public static T get<T>(TreeDict tree, string memberName, T defaultValue = default)
        {
            if (tree == null) return defaultValue;
            if (!tree.TryGetValue(memberName, out JToken result))
                return defaultValue;
            try
            {
                return result.ToObject<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// The extension method form of <see cref="get{T}(TreeDict, string, T)"/>.
        /// </summary>
        /// <seealso cref="get{T}(TreeDict, string, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(this TreeDict t, string m, T d = default) => get(t, m, d);

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// Use <see cref="get{T}(TreeDict, string, T)"/> unless <typeparamref name="T"/> is a value type and you want a <see cref="Nullable{T}"/> as a result.
        /// </summary>
        /// <typeparam name="T">The type of the member to be returned</typeparam>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        public static T? getNullable<T>(TreeDict tree, string memberName, T? defaultValue = null) where T : struct
        {
            if (tree == null) return defaultValue;
            if (!tree.TryGetValue(memberName, out JToken result))
                return defaultValue;
            try
            {
                return (T?)result.ToObject<T>() ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// The extension method form of <see cref="getNullable{T}(TreeDict, string, T?)"/>.
        /// </summary>
        /// <seealso cref="getNullable{T}(TreeDict, string, T?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetNullable<T>(this TreeDict t, string m, T? d = default) where T : struct => getNullable(t, m, d);

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// Use <see cref="get{T}(object, string, T)"/> unless the member you're getting is a Tree.
        /// </summary>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        [Obsolete] // Fern doesn't suggest using this, this will return defaultValue or JToken
        public static object getTree(TreeDict tree, string memberName, object defaultValue = null)
        {
            if (tree == null) return defaultValue;
            if (!tree.TryGetValue(memberName, out JToken result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// If <typeparamref name="T"/> is a value type and you want <paramref name="defaultValue"/> to be null, use <see cref="getNullable{T}(object, string, T?)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the member to be returned</typeparam>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        public static T get<T>(object tree, string memberName, T defaultValue = default)
        {
            try
            {
                if ((TreeDict)tree == null) return defaultValue;
                return get((TreeDict)tree, memberName, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// The extension method form of <see cref="get{T}(object, string, T)"/>.
        /// </summary>
        /// <seealso cref="get{T}(object, string, ?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(object t, string m, T d = default) => get(t, m, d);

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// Use <see cref="get{T}(object, string, T)"/> unless <typeparamref name="T"/> is a value type and you want a <see cref="Nullable{T}"/> as a result.
        /// </summary>
        /// <typeparam name="T">The type of the member to be returned</typeparam>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        public static T? getNullable<T>(object tree, string memberName, T? defaultValue = null) where T : struct
        {
            try
            {
                if ((TreeDict)tree == null) return defaultValue;
                return getNullable((TreeDict)tree, memberName, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// The extension method form of <see cref="getNullable{T}(object, string, T?)"/>.
        /// </summary>
        /// <seealso cref="getNullable{T}(object, string, T?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? GetNullable<T>(object t, string m, T? d = default) where T : struct => getNullable(t, m, d);

        /// <summary>
        /// Safely accesses a member of a tree, returning <paramref name="defaultValue"/> if the member does not exist or if <paramref name="tree"/> is null.
        /// Use <see cref="get{T}(object, string, T)"/> unless the member you're getting is a Tree.
        /// </summary>
        /// <param name="tree">The tree to access a member of</param>
        /// <param name="memberName">The name of the member to be accessed</param>
        /// <param name="defaultValue">The value to be returned if <paramref name="tree"/> is null or has no member <paramref name="memberName"/></param>
        /// <returns>The value of <paramref name="tree"/>'s member <paramref name="memberName"/>, or <paramref name="defaultValue"/> if tree is null or no such member exists</returns>
        public static object getTree(object tree, string memberName, object defaultValue = null)
        {
            try
            {
                if ((TreeDict)tree == null) return defaultValue;
                return getTree((TreeDict)tree, memberName, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Combines the contents of the two trees, recursively merging subtrees where appropriate.
        /// When both Trees contain members of the same name:
        /// - If that member is of the same underlying type as a Tree on both arguments, those Trees will be merged with the same priorities as the Trees they were members of.
        /// - Otherwise, the member from highPriority will be used.
        /// If either Tree is null, returns the other.
        /// If both are null, returns null.
        /// </summary>
        /// <param name="highPriority">The Tree to keep the members of in case of conflict</param>
        /// <param name="lowPriority">The Tree to drop the members of in case of conflict</param>
        /// <param name="copySubtrees">If true, all Trees encountered as members in only one of highPriority and lowPriority will be copied as with copy(Tree) rather than copied by reference.
        /// Increases performance cost by the cost of those copies.</param>
        /// <returns>A tree consisting of highPriority and lowPriority merged</returns>
        public static TreeDict mergeTrees(TreeType highPriority, TreeType lowPriority, bool copySubtrees = true)
        {
            if (highPriority == null) return lowPriority;
            if (lowPriority == null) return highPriority;
            TreeDict result = copySubtrees ? copy(lowPriority) : shallowCopy(lowPriority);
            foreach (KeyValuePair<string, JToken> pair in highPriority)
            {
                if (pair.Value is TreeType)
                {
                    if (at(lowPriority, pair.Key) is TreeType)
                    {
                        result[pair.Key] = mergeTrees(pair.Value as TreeType, ((TreeDict)lowPriority)[pair.Key] as TreeType);
                    }
                    else result[pair.Key] = copySubtrees ? copy(pair.Value as TreeType) : pair.Value;
                }
                else
                {
                    result[pair.Key] = pair.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// An extension method form of <see cref="mergeTrees(TreeType, TreeType, bool)"/>.
        /// </summary>
        /// <seealso cref="mergeTrees(TreeType, TreeType, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeDict Merge(this TreeType highPriority, TreeType lowPriority, bool copySubtrees = true) => mergeTrees(highPriority, lowPriority, copySubtrees);

        /// <summary>
        /// Maps an <see cref="Action{Tree}"/> over a tree and its subtrees.
        /// <paramref name="action"/> will first be called on <paramref name="tree"/>, then on each subtree as it's encountered in a depth-first search.
        /// </summary>
        /// <param name="tree">The tree to map over</param>
        /// <param name="action">The Action to map over the tree</param>
        public static void map(TreeType tree, Action<TreeDict> action)
        {
            action(tree);
            foreach (KeyValuePair<string, JToken> pair in tree)
            {
                if (pair.Value is TreeType t)
                {
                    map(t, action);
                }
            }
        }

        /// <summary>
        /// An extension method form of <see cref="map(TreeType, Action{object})"/>.
        /// </summary>
        /// <seealso cref="map(TreeType, Action{object})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Map(this TreeType t, Action<TreeDict> a) => map(t, a);

        /// <summary>
        /// Maps a <see cref="Func{Tree, Tree}"/> over <paramref name="tree"/> and its subtrees, replacing those subtrees with the result of mapping <paramref name="func"/> over them.
        /// If <paramref name="doCopy"/> is true, the Trees <paramref name="func"/> is applied to will be <see cref="copy(TreeType)">copies</see> of <paramref name="tree"/> and its subtrees.
        /// </summary>
        /// <param name="tree">The tree to map over.</param>
        /// <param name="func">The transformation to apply to each subtree.</param>
        /// <param name="doCopy">Whether to copy the tree before transforming it.</param>
        /// <returns></returns>
        public static TreeDict map(TreeType tree, Func<TreeDict, TreeType> func, bool doCopy = true)
        {
            TreeType newTree = func(doCopy ? copy(tree) : tree);
            List<string> treeKeys = new List<string>();
            foreach (KeyValuePair<string, JToken> pair in newTree)
            {
                if (pair.Value is TreeType)
                {
                    treeKeys.Add(pair.Key);
                }
            }
            foreach (string key in treeKeys)
            {
                ((TreeDict)newTree)[key] = map(((TreeDict)newTree)[key] as TreeType, func, false);
            }
            return newTree;
        }

        /// <summary>
        /// An extension method form of <see cref="map(TreeType, Func{TreeDict, TreeType}, bool)"/>.
        /// </summary>
        /// <seealso cref="map(TreeType, Func{TreeDict, TreeType}, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Map(this TreeType t, Func<TreeDict, TreeType> f, bool c = true) => map(t, f, c);

        /// <summary>
        /// Checks whether an object is of the right type to be a Tree. Does *not* validate whether the object is a *valid* Tree; use <see cref="isTree(object)"/> for this purpose.
        /// </summary>
        /// <param name="o">The object to typecheck.</param>
        /// <returns>Whether <paramref name="o"/> is of the right type to be a Tree.</returns>
        public static bool isTreeType(object o)
        {
            return o is TreeType;
        }

        /// <summary>
        /// An alternate name of <see cref="isTreeType(object)"/>.
        /// </summary>
        /// <seealso cref="isTreeType(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTreeType(object o) => isTreeType(o);

        /// <summary>
        /// Checks whether an object is a valid Tree. (Valid Trees are all of the same underlying type, and do not contain themselves directly or indirectly.) This involves recursively validating
        /// all subtrees; if performance is critical, consider whether <see cref="isTreeType(object)"/> suits your needs better.
        /// </summary>
        /// <param name="o">The object to validate.</param>
        /// <returns>Whether <paramref name="o"/> meets all the criteria to be a Tree.</returns>
        public static bool isTree(object o)
        {
            // Validates a member of a Tree (or a Tree itself) to make sure it doesn't contain itself or any of its parent Trees.
            // ACCUMULATOR: parents
            // Remembers: all trees that are parents of child
            bool isTreeInternal(object child, HashSet<object> parents)
            {
                // no need to validate non-TreeTypes
                if (!isTreeType(child)) return true;
                // if this child is one of its own parents, there's a subtree loop; invalid tree
                if (parents.Contains(child)) return false;

                // update accumulator
                HashSet<object> newSeen = new HashSet<object> { child };
                newSeen.UnionWith(parents);

                // if all subtrees are valid, this tree is valid
                return (child as TreeDict).All<KeyValuePair<string, JToken>>(p => isTreeInternal(p.Value, newSeen));
            }

            return isTreeType(o) && isTreeInternal(o, new HashSet<object>());
        }

        /// <summary>
        /// An alternate name of <see cref="isTree(object)"/>.
        /// </summary>
        /// <seealso cref="isTree(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTree(object o) => isTree(o);
    }
}
