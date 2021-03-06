﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10TextureDescriptorGenerator : ITextureDescriptorGenerator
    {
        private readonly GltfParser m_parser;
        private TextureDescriptorSet _textureDescriptorSet;

        public Vrm10TextureDescriptorGenerator(GltfParser parser)
        {
            m_parser = parser;
        }

        public TextureDescriptorSet Get()
        {
            if (_textureDescriptorSet == null)
            {
                _textureDescriptorSet = new TextureDescriptorSet();
                foreach (var (_, param) in EnumerateAllTextures(m_parser))
                {
                    _textureDescriptorSet.Add(param);
                }
            }
            return _textureDescriptorSet;
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーを列挙する
        /// </summary>
        private static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfParser parser)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            // Textures referenced by Materials.
            for (var materialIdx = 0; materialIdx < parser.GLTF.materials.Count; ++materialIdx)
            {
                var m = parser.GLTF.materials[materialIdx];
                if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions, out var mToon))
                {
                    foreach (var (_, tex) in Vrm10MToonTextureImporter.EnumerateAllTextures(parser, m, mToon))
                    {
                        yield return tex;
                    }
                }
                else
                {
                    // Fallback to glTF PBR & glTF Unlit
                    foreach (var tex in GltfPbrTextureImporter.EnumerateAllTextures(parser, materialIdx))
                    {
                        yield return tex;
                    }
                }
            }

            // Thumbnail Texture referenced by VRM Meta.
            if (TryGetMetaThumbnailTextureImportParam(parser, vrm, out (SubAssetKey key, TextureDescriptor) thumbnail))
            {
                yield return thumbnail;
            }
        }

        /// <summary>
        /// VRM-1 の thumbnail テクスチャー。gltf.textures ではなく gltf.images の参照であることに注意(sampler等の設定が無い)
        /// </summary>
        public static bool TryGetMetaThumbnailTextureImportParam(GltfParser parser, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, out (SubAssetKey, TextureDescriptor) value)
        {
            if (vrm?.Meta?.ThumbnailImage == null)
            {
                value = default;
                return false;
            }

            var imageIndex = vrm.Meta.ThumbnailImage.Value;
            var gltfImage = parser.GLTF.images[imageIndex];
            var name = TextureImportName.GetUnityObjectName(TextureImportTypes.sRGB, gltfImage.name, gltfImage.uri);

            GetTextureBytesAsync getThumbnailImageBytesAsync = () =>
            {
                var bytes = parser.GLTF.GetImageBytes(parser.Storage, imageIndex);
                return Task.FromResult(GltfTextureImporter.ToArray(bytes));
            };
            var texDesc = new TextureDescriptor(name, gltfImage.GetExt(), gltfImage.uri, Vector2.zero, Vector2.one, default, TextureImportTypes.sRGB, default, default,
               getThumbnailImageBytesAsync, default, default,
               default, default, default
               );
            value = (texDesc.SubAssetKey, texDesc);
            return true;
        }
    }
}
