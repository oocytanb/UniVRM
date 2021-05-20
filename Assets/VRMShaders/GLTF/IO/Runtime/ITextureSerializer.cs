﻿using UnityEngine;

namespace UniGLTF
{
    public interface ITextureSerializer
    {
        /// <summary>
        /// Texture をファイルのバイト列そのまま出力してよいかどうか判断するデリゲート。
        ///
        /// Runtime 出力では常に false が望ましい。
        /// </summary>
        bool CanExportAsEditorAssetFile(Texture texture);

        /// <summary>
        /// Texture2D から実際のバイト列を取得するデリゲート。
        ///
        /// textureColorSpace はその Texture2D がアサインされる glTF プロパティの仕様が定める色空間を指定する。
        /// 具体的には Texture2D をコピーする際に、コピー先の Texture2D の色空間を決定するために使用する。
        /// </summary>
        (byte[] bytes, string mime) ExportBytesWithMime(Texture2D texture, ColorSpace textureColorSpace);
    }
}
