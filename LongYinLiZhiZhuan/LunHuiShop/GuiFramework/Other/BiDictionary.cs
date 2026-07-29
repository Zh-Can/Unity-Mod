namespace LunHuiShop.GuiFramework.Other;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 双向字典，支持通过键或值快速查找对方，并保证键和值的唯一性。
/// 实现 <see cref="IReadOnlyDictionary{TKey, TValue}"/> 接口，可与 LINQ 等无缝协作。
/// </summary>
/// <typeparam name="TKey">键的类型（必须非空）</typeparam>
/// <typeparam name="TValue">值的类型（必须非空）</typeparam>
public sealed class BiDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    private const string DuplicateKeyMsg = "Duplicate key";
    private const string DuplicateValueMsg = "Duplicate value";

    private readonly Dictionary<TKey, TValue> _forward;
    private readonly Dictionary<TValue, TKey> _reverse;
    private readonly IEqualityComparer<TKey> _keyComparer;
    private readonly IEqualityComparer<TValue> _valueComparer;

    // ---------- 构造函数 ----------
    /// <summary>
    /// 使用默认比较器，初始容量为默认值。
    /// </summary>
    public BiDictionary()
        : this(0, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// 指定初始容量，使用默认比较器。
    /// </summary>
    /// <param name="capacity">初始容量（必须 >= 0）</param>
    public BiDictionary(int capacity)
        : this(capacity, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// 使用自定义比较器，初始容量为默认值。
    /// </summary>
    public BiDictionary(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        : this(0, keyComparer, valueComparer)
    {
    }

    /// <summary>
    /// 指定初始容量和自定义比较器。
    /// </summary>
    /// <param name="capacity">初始容量（必须 >= 0）</param>
    /// <param name="keyComparer">键的比较器（为 null 则使用默认）</param>
    /// <param name="valueComparer">值的比较器（为 null 则使用默认）</param>
    public BiDictionary(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");

        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

        _forward = new Dictionary<TKey, TValue>(capacity, _keyComparer);
        _reverse = new Dictionary<TValue, TKey>(capacity, _valueComparer);
    }

    // ---------- 基本属性 ----------
    /// <summary>获取键/值对的数量。</summary>
    public int Count => _forward.Count;

    /// <summary>获取所有键的集合（唯一）。</summary>
    public IEnumerable<TKey> Keys => _forward.Keys;

    /// <summary>获取所有值的集合（唯一）。</summary>
    public IEnumerable<TValue> Values => _reverse.Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    // ---------- 索引器 ----------
    /// <summary>通过键获取或设置值（设置时使用覆盖语义）若 value 已被其他 key 占用，会移除旧映射。。</summary>
    public TValue this[TKey key]
    {
        get => _forward[key];
        set => AddOrReplace(key, value);
    }

    /// <summary>通过值获取键（若不存在则引发异常）。</summary>
    public TKey GetKeyByValue(TValue value) => _reverse[value];

    // ---------- 查询方法 ----------
    /// <summary>是否包含指定键。</summary>
    public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

    /// <summary>是否包含指定值。</summary>
    public bool ContainsValue(TValue value) => _reverse.ContainsKey(value);

    /// <summary>尝试获取键对应的值。</summary>
    public bool TryGetValue(TKey key, out TValue value)
        => _forward.TryGetValue(key, out value);

    /// <summary>尝试获取值对应的键。</summary>
    public bool TryGetKey(TValue value, out TKey key)
        => _reverse.TryGetValue(value, out key);

    // ---------- 添加方法 ----------
    /// <summary>
    /// 严格添加：键和值都必须唯一，否则抛出异常。
    /// 操作是原子的：若值冲突会回滚正向插入。
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        if (!_forward.TryAdd(key, value))
            throw new ArgumentException(DuplicateKeyMsg, nameof(key));

        if (!_reverse.TryAdd(value, key))
        {
            _forward.Remove(key);
            throw new ArgumentException(DuplicateValueMsg, nameof(value));
        }
    }

    /// <summary>
    /// 尝试添加，若键或值已存在则返回 false，不引发异常。
    /// 操作是原子的：若值冲突会回滚正向插入。
    /// </summary>
    public bool TryAdd(TKey key, TValue value)
    {
        if (!_forward.TryAdd(key, value))
            return false;

        if (!_reverse.TryAdd(value, key))
        {
            _forward.Remove(key);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 添加或覆盖：如果键已存在则覆盖旧值，如果值已被其他键占用则移除那个旧键，
    /// 保证最终映射唯一。若键值对已完全相同（使用自定义比较器）则无操作。
    /// </summary>
    public void AddOrReplace(TKey key, TValue value)
    {
        // 若完全相同的映射已存在，直接返回（使用自定义比较器）
        if (_forward.TryGetValue(key, out var oldVal) && _valueComparer.Equals(oldVal, value))
            return;

        // 移除可能冲突的旧映射
        if (_forward.TryGetValue(key, out var existingVal))
            _reverse.Remove(existingVal);

        if (_reverse.TryGetValue(value, out var existingKey))
            _forward.Remove(existingKey);

        _forward[key] = value;
        _reverse[value] = key;
    }

    // ---------- 删除方法 ----------
    /// <summary>按键删除，成功返回 true。</summary>
    public bool Remove(TKey key)
    {
        if (!_forward.Remove(key, out var value))
            return false;
        _reverse.Remove(value);
        return true;
    }

    /// <summary>按值删除，成功返回 true。</summary>
    public bool RemoveByValue(TValue value)
    {
        if (!_reverse.Remove(value, out var key))
            return false;
        _forward.Remove(key);
        return true;
    }

    /// <summary>清空所有映射。</summary>
    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    // ---------- 枚举支持 ----------
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => _forward.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

//
// // 使用默认配置
// var dict = new BiDictionary<string, int>();
//
// // 指定容量（优化大量数据）
// var dict2 = new BiDictionary<string, int>(1000);
//
// // 使用自定义比较器（忽略键大小写，值区分大小写）
// var dict3 = new BiDictionary<string, string>(
//     StringComparer.OrdinalIgnoreCase,
//     EqualityComparer<string>.Default
// );
//
// // 同时指定容量和比较器
// var dict4 = new BiDictionary<string, string>(
//     500,
//     StringComparer.OrdinalIgnoreCase,
//     StringComparer.OrdinalIgnoreCase
// );
//
// // 基本操作
// dict.Add("one", 1);
// dict.AddOrReplace("two", 2);
// dict.TryAdd("three", 3);
//
// bool hasKey = dict.ContainsKey("one");
// bool hasValue = dict.ContainsValue(2);
//
// int val = dict["one"];        // 1
// string key = dict.GetKeyByValue(2); // "two"
//
// // 遍历
// foreach (var kv in dict)
//     Console.WriteLine($"{kv.Key} -> {kv.Value}");
//
// // 删除
// dict.Remove("one");
// dict.RemoveByValue(3);