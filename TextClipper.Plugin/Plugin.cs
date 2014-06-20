using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextClipper.Plugin
{
    /// <summary>
    /// TextClipperのプラグインのインターフェースを定義します。
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// プラグインの名前を取得します。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// プラグインの説明を取得します。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 表示されるイメージのImageSourceを取得します。
        /// TextClipper.Plugin内にある拡張子 ToImageSource を利用することができます。
        /// </summary>
        System.Windows.Media.ImageSource Icon { get; }

        /// <summary>
        /// TextClipperが終了する時に呼び出されます。
        /// </summary>
        void Exit();
    }

    /// <summary>
    /// テキスト入力時に呼び出される関数を定義します。
    /// </summary>
    public interface ISupportInput
    {
        /// <summary>
        /// テキストがクリップボードから入力される時に呼び出されます。
        /// </summary>
        /// <param name="value">入力される元テキスト</param>
        /// <returns>入力するテキスト</returns>
        string Inputting(string value);
    }

    /// <summary>
    /// テキスト出力時に呼び出される関数を定義します。
    /// </summary>
    public interface ISupportOutput
    {
        /// <summary>
        /// テキストがクリップボードに出力される時に呼び出されます。
        /// </summary>
        /// <param name="value">出力される元テキスト</param>
        /// <returns>出力するテキスト</returns>
        string Outputting(string value);
    }

}
