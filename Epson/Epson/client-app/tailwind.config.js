

const plugin = require('tailwindcss/plugin')




module.exports = {
    purge: [],
    darkMode: false, // or 'media' or 'class'
    theme: {
     
      
        fontFamily: {
            'sans': ['TCCC-UnityText-Regular', 'TCCC-UnityText']
           },
      extend: {
        spacing: {
          '13': '3.25rem', // 52px
          '15': '3.75rem', // 60px
          '18': '4.5rem', // 72px
          '22': '5.5rem', // 88px
          '26': '6.5rem', // 104px
          '30': '7.5rem',
          '34': '8.5rem',
        },
        backgroundImage: {
          'body-texture-2': "url('/images/png/group-23334@2x.jpg')",
          'body-frame-1': "url('/images/png/bg.png')",
          'body-frame-2': "url('/images/png/Group 23156@3x.png')",
          'body-frame-3': "url('/images/png/lotus/steps-form-frame@3x2.png')",
          'body-frame-bottom-3': "url('/images/png/lotus/steps-form-frame-bottom@3x.png')",
          'body-frame-middle-3': "url('/images/png/lotus/steps-form-frame-middle@3.png')"
          
        },
        
      // that is animation class
      animation: {
        fade: 'fadeIn 3s ease-in-out',
        loader: 'loader 0.6s alternate',
        bouncetat: 'bouncetat 1s'
      },
      // that is actual animation
      keyframes: theme => ({

        fadeIn: {
          '0%': { backgroundColor: theme('opacity.0') },
          '50%': { backgroundColor: theme('opacity.0') },
          '100%': { backgroundColor: theme('opacity.100') },
        },
         
        loader: {
          
          to: {
            opacity: 0.1,
            transform: 'translate3d(0, -1rem, 0)'
          }
        },

        bouncetat: {
          '0%' :{
            opacity: '0.1',
            transform: 'translateY(-25%)',
            animationTimingFunction: 'cubic-bezier(0.8, 0, 1, 1)'
          },
          '100%': {
            opacity: '1.0',
            transform: 'translateY(0)',
            animationTimingFunction: 'cubic-bezier(0, 0, 0.2, 1)'
          }
        }
      }),
        
      }
    },

    
    variants: {
      extend: {},
      opacity: ['disabled'],
      cursor: ['disabled'],
    },
    plugins: [
      require('@tailwindcss/forms'),
      require('tailwindcss'),
      require('autoprefixer'),
      require('postcss-nested')
    ],
    /**
    plugins: [
        require('@tailwindcss/forms'),
        plugin(function({ addComponents, theme }) {
            const buttons = {
              '.btn': {
                padding: `${theme('spacing.2')} ${theme('spacing.4')}`,
                borderRadius: theme('borderRadius.md'),
                fontWeight: theme('fontWeight.600'),
              },
              '.btn-indigo': {
                backgroundColor: theme('colors.indigo.500'),
                color: theme('colors.white'),
                '&:hover': {
                  backgroundColor: theme('colors.indigo.600')
                },
              },
            }
      
            addComponents(buttons)
          })
    ],
    */
  }

  

