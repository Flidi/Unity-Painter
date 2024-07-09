Shader "Custom/DecalMaskShader" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1) // Колір для текстури
        _masktex ("Albedo (RGB)", 2D) = "white" {} // Текстура альбедо
        _Glossiness ("Smoothness", Range(0,1)) = 0.5 // Гладкість
        _Metallic ("Metallic", Range(0,1)) = 0.0 // Металічність
        _hitposx("posx",float)=1.0 // Позиція X для удару
        _hitposy("posy",float)=1.0 // Позиція Y для удару
        _hitposz("posz",float)=1.0 // Позиція Z для удару
        _BrushSize("Brush Size", float) = 0.1 // Розмір кисті
        _BrushColor("Brush Color", Color) = (1,0,0,1) // Колір кисті
    }
    SubShader {
        Tags { "RenderType"="Opaque" } // Теги для підшейдера
        Pass {
            CGPROGRAM
            #pragma vertex vert // Вершинний шейдер: vert
            #pragma fragment frag // Фрагментний шейдер: frag

            sampler2D _masktex; // Семплер для текстури альбедо

            // Вхідні дані для вершинного шейдера
            struct vertin
            {
                float4 vertex : POSITION; // Позиція вершини
                float2 uv : TEXCOORD0; // UV-координати
            };

            // Вихідні дані з вершинного шейдера
            struct vertout
            {
                float4 pos : SV_POSITION; // Позиція на екрані
                float2 uv : TEXCOORD0; // Передача UV-координат
                float4 worldpos : TEXCOORD1; // Передача світової позиції
            };

            // Вершинний шейдер
            vertout vert(vertin v)
            {
                vertout o;
                o.worldpos = mul(unity_ObjectToWorld, v.vertex); // Обчислення світової позиції
                o.uv = v.uv; // Передача UV-координат
                v.uv = v.uv * 2.0 - 1.0; // Масштабування UV-координат
                v.vertex.x = v.uv.x; // Оновлення координати X вершини
                v.vertex.y = -v.uv.y; // Оновлення координати Y вершини
                v.vertex.z = 1; // Задання координати Z вершини
                o.pos = v.vertex; // Передача позиції
                return o;
            }
            uniform float _hitposx;
            uniform float _hitposy;
            uniform float _hitposz;
            uniform float _BrushSize;
            uniform float4 _BrushColor;
            float4 frag(vertout i) : COLOR {
               float3 mhitpos; // Локальна змінна для позиції удару
                mhitpos.x = _hitposx; // Зчитування позиції X удару
                mhitpos.y = _hitposy; // Зчитування позиції Y удару
                mhitpos.z = _hitposz; // Зчитування позиції Z удару
                fixed4 col = tex2D(_masktex, i.uv); // Зчитування кольору з текстури альбедо

                // Перевірка, чи відстань між позицією удару і світовою позицією не більша за розмір кисті
                if (distance(mhitpos, i.worldpos.xyz) < _BrushSize)
                {
                    // Змішування кольору з кольором кисті в залежності від альфа-каналу кольору кисті
                    return lerp(col, _BrushColor, _BrushColor.a);
                }

                return col; // Повернення оригінального кольору, якщо умова не виконана
            }
            ENDCG // Кінець CG-блоку
        }
    }

    FallBack "Diffuse" // Повернення до стандартного шейдера "Diffuse" як резервний варіант
}