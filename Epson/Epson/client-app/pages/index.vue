<template>

  <div id="cokemain" class="bg-body-texture-2 bg-cover bg-blend-luminosity bg-fixed relative  text-base">
    <VueTailwindModal :showing="showingPrivacy" @update:showing="showingPrivacy = $event">
        <coke-privacy />
    </VueTailwindModal>

    <VueTailwindModal :showing="showing" @update:showing="showing = $event" @close="close()">
        <coke-terms />
    </VueTailwindModal>

              
        <VueTailwindModal :showing="isShowingStores" @update:showing="isShowingStores = $event" >
          <div class="flex flex-col text-left
          py-3 lg:py-16 
          px-3 lg:px-24 
          text-black
          ">
                <div class="h1 font-black flex items-center text-lg md:text-lg lg:text-xl 2xl:text-2xl" >
                  Participating Stores </div>


                  <div class="w-full text-sm sm:text-base">
                    <div class="m-8">
                      <ol class="list-decimal">

                        <li>7-Eleven</li>
                        <li>FamilyMart</li>
                        <li>myNEWS</li>
                        <li>BHP BHPetrolmart</li>
                        <li>Caltex Xpresstore</li>
                        <li>Petronas Mesra</li>
                        <li>Petron Treats</li>
                      </ol> 
                    </div>
                  </div>
          </div>
        </VueTailwindModal>

    <nav class="flex items-center justify-between w-full mx-auto  p-6

     fixed z-30
          inset-x-0
          top-0
          mb-3
          bg-body-texture-2  bg-cover
          flex-wrap
          space-x-4
          transition-shadow
          " :class="{'shadow-lg':isUserScrolling}">
      <div class="
    items-center justify-around lg:px-6 xl:px-20 2xl:px-48
    w-full hidden lg:flex
    ">
        <a v-scroll-to="{ el: '#howtoparticipate', offset: -50 }" class="w-1/3 block mt-4 lg:inline-block lg:mt-0 text-white text-center hover:text-white  cursor-pointer">
          How to participate
        </a>

        <img src="images/png/whitelogo.png" class="w-36 md:w-48" alt="" />

        <a class="w-1/3  block mt-4 lg:inline-block lg:mt-0  text-white hover:text-white cursor-pointer text-center" @click="openModal">
          Terms and Conditions
        </a>


      </div>

      <div class="flex justify-between w-full lg:hidden">

        <img src="images/png/whitelogo.png" class="w-36 md:w-48" alt="" />
        <button class="text-gray-500 w-10 h-10 relative focus:outline-none bg-transparent" @click="open=!open">
          <span class="sr-only">Open main menu</span>
          <div class="block w-5 absolute left-1/2 top-1/2   transform  -translate-x-1/2 -translate-y-1/2">
            <span aria-hidden="true" class="block absolute h-0.5 w-5 bg-white transform transition duration-500 ease-in-out"
                  :class="{'rotate-45': open,' -translate-y-1.5': !open }"></span>
            <span aria-hidden="true" class="block absolute  h-0.5 w-5 bg-white   transform transition duration-500 ease-in-out" :class="{'opacity-0': open } "></span>
            <span aria-hidden="true" class="block absolute  h-0.5 w-5 bg-white transform  transition duration-500 ease-in-out" :class="{'-rotate-45': open, ' translate-y-1.5': !open}"></span>
          </div>
        </button>
      </div>

      <div class="w-full block flex-grow  lg:items-center lg:w-auto lg:hidden" :class="{'hidden': !open } ">
        <div class="text-sm lg:flex-grow">
          <a v-scroll-to="{ el: '#howtoparticipate', offset: -50 }" class=" block mt-4 lg:inline-block lg:mt-0 text-white hover:text-white mr-4 cursor-pointer">
            How to participate 
          </a>
          <a class="block mt-4 lg:inline-block lg:mt-0 text-white hover:text-white" @click="openModal">
            Terms and Conditions
          </a>
        </div>
        <!--
        <div>
          <a href="#" class="inline-block text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-teal-500 hover:bg-white mt-4 lg:mt-0">Download</a>
        </div>
        -->
      </div>
    </nav>












    <div class="container mx-auto relative">
      <img src="images/png/Group 23191@2x.png" class="absolute top-24  right-0" style="width:800px;" alt="" />
      <img src="images/png/Group 23201@3x.png" class="absolute -bottom-96 left-0" style="width:450px;" alt="" />

      <div class="flex flex-col justify-center items-center pt-24 md:pt-28 sm:px-20 xl:px-20 2xl:px-40">


        <div class="hidden mt-1 sm:block">
          <img src="images/png/Group 23184@3x.png" class="w-36 md:w-48" alt="" />
        </div>








        <div class="z-10 xl:px-44 2xl:px-64 mb-16 px-4">
          <div class="font-bold  mt-16 mb-6 text-white text-center">Contest period: 1<sup>st</sup>&nbsp;Jan 2022 - 28<sup>th</sup>&nbsp;Feb 2022 </div>
          <img src="images/png/888/rm888@3x.png" class="w-full lg:w-2/3 xl:w-auto mx-auto" alt="" />

        </div>

        <div class="overflow-x-hidden md:overflow-x-visible">
          <div class="-mx-36 md:-mx-14 xl:mx-auto transform  text-sm md:text-base
        small-brown-bg sm:small-brown-bg-none
        ">


            <img src="images/png/bg.png" class="hidden sm:block" alt="" />

            <div class="sm:absolute sm:inset-0 mx-2 sm:mx-40 md:mx-auto top-4 lg:top-16 flex flex-col justify-start items-center">




             <div class="hidden sm:block">
              <div class="w-full flex flex-row justify-around items-start mt-8 px-30">
                  <div class="w-3/12 h-full flex flex-col justify-center items-center relative">
                  </div>
                  <div class="w-3/12  h-full flex flex-col justify-end items-center relative">
                    <div class="flex flex-col items-center text-center animate-fade">
                      <div class="lg:text-2xl h1 font-black">1st Prize x 20*</div>
                    </div>
                  </div>
                  <div class="w-3/12   h-full  flex  flex-col justify-end items-center relative">
                    <div class="flex flex-col items-center animate-fade">
                      <div class="lg:text-2xl h1 font-black">2nd Prize x 80*</div>
                    </div>
                  </div>
                  <div class="w-3/12 h-full flex  flex-col justify-end items-center relative">
                    <div class="flex flex-col text-center items-center animate-fade ">
                      <div class="lg:text-2xl h1 font-black">3rd Prize x 300*</div>
                    </div>
                  </div>
              </div>
              <div class="flex flex-row justify-around items-start mt-8 px-30">
                  <div class="w-3/12 px-2 lg:px-10 h-full flex flex-col justify-center items-center relative">
                    <img src="images/png/888/tng888@3x.png"   class=" animate-bouncetat transition duration-150 ease-in-out cursor-pointer transform hover:scale-110" alt="" />
                  </div>
                  <div class="w-3/12  px-2 lg:px-10 h-full flex flex-col justify-end items-center relative">
                    <img src="images/png/888/prize888@3x.png"  class=" animate-bouncetat transition duration-150 ease-in-out cursor-pointer transform hover:scale-110" alt="" />
                  
                  </div>
                  <div class="w-3/12  px-2 lg:px-10  h-full  flex  flex-col justify-end items-center relative">
                    <img src="images/png/888/prize88@3x.png" class=" animate-bouncetat  transition duration-150 ease-in-out animation-delay-400  transform  cursor-pointer hover:scale-110" alt="" />
                  
                  </div>
                  <div class="w-3/12 px-2 lg:px-10 h-full flex  flex-col justify-end items-center relative">
                    <img src="images/png/888/prize8@3x.png" class="  animate-bouncetat transition duration-150 ease-in-out animation-delay-200  transform cursor-pointer hover:scale-110" alt="" />
                  
                  </div>
              </div>
             </div>

             <div class="block sm:hidden">
              <div class="mx-auto w-24">
                  <img src="images/png/888/tng888@3x.png"   class=" animate-bouncetat transition duration-150 ease-in-out cursor-pointer transform hover:scale-110" alt="" />
              </div>
              <div class="w-full flex flex-row justify-around items-start mt-8 px-36">
                  <div class="w-4/12  h-full flex flex-col justify-end items-center relative">
                    <div class="flex flex-col items-center text-center animate-fade">
                      <div class="lg:text-2xl h1 font-black">1st Prize<br/> x 20*</div>
                    </div>
                  </div>
                  <div class="w-4/12   h-full  flex  flex-col justify-end items-center relative">
                    <div class="flex flex-col items-center text-center animate-fade">
                      <div class="lg:text-2xl h1 font-black">2nd Prize<br/> x 80*</div>
                    </div>
                  </div>
                  <div class="w-4/12 h-full flex  flex-col justify-end items-center relative">
                    <div class="flex flex-col text-center items-center animate-fade ">
                      <div class="lg:text-2xl h1 font-black">3rd Prize<br/> x 300*</div>
                    </div>
                  </div>
              </div>
              <div class="flex flex-row justify-around items-start mt-8 px-36">

                  <div class="w-4/12  px-2 lg:px-10 h-full flex flex-col justify-end items-center relative">
                    <img src="images/png/888/prize888@3x.png"  class=" animate-bouncetat transition duration-150 ease-in-out cursor-pointer transform hover:scale-110" alt="" />
                  
                  </div>
                  <div class="w-4/12  px-2 lg:px-10  h-full  flex  flex-col justify-end items-center relative">
                    <img src="images/png/888/prize88@3x.png" class=" animate-bouncetat  transition duration-150 ease-in-out animation-delay-400  transform  cursor-pointer hover:scale-110" alt="" />
                  
                  </div>
                  <div class="w-4/12 px-2 lg:px-10 h-full flex  flex-col justify-end items-center relative">
                    <img src="images/png/888/prize8@3x.png" class="  animate-bouncetat transition duration-150 ease-in-out animation-delay-200  transform cursor-pointer hover:scale-110" alt="" />
                  
                  </div>
              </div>
             </div>
            <div class="my-2">* Terms and Conditions apply</div>
    <div  id="howtoparticipate" class="container mx-auto pt-16 xl:px-30 2xl:px-40 relative ">

      <div class="flex flex-col justify-center items-center">
        <div class="h1 font-black  flex mt-4 items-center text-center text-xl lg:text-3xl xl:text-4xl" >How to participate</div>
      </div>

      <div class="flex flex-col md:flex-row px-6 md:px-0 justify-around mt-4 lg:mt-10 xl:mt-16 xl:mb-4 ">
        <div class="flex-1 flex flex-col justify-center items-center  text-center p-4 md:p-0 relative">
          <img src="images/png/888/step1b@3x.png" class="w-16" alt="" />
          <div class="mt-8 mb-4 font-bold">Step 1</div>
          <div class="h-auto md:h-24 px-36 sm:px-0">Purchase <span class="text-2xl font-bold">RM15</span> of any participating brands
          from <a class="text-base underline cursor-pointer" @click="isShowingStores=!isShowingStores">Participating Stores</a>
          
          </div>

   

        </div>
        <div class="flex-1 flex flex-col justify-center items-center text-center p-4 md:p-0">
          <img src="images/png/888/step2b@3x.png" class="w-16" alt="" />
          <div class="mt-8 mb-4 font-bold">Step 2</div>
          <div class="h-auto md:h-24  px-32 sm:px-0">Click 

          <a v-scroll-to="{ el: '#submitsection', offset: -50 }" class="  bg-red-600 text-white border border-white hover:border-red-200
                    ml-2 cursor-pointer
                  px-2 md:px-8 
                  py-1 
                  rounded-full">
          Participate Now
        </a>


                 <!-- <button  v-scroll-to="'#submitsection'" class="   bg-red-600 text-white border border-white hover:border-red-200
                    ml-2
                  px-2 md:px-8 
                  py-1 
                  rounded-full" >Participate Now</button>-->
                  </div> 
        </div>

        <div class="flex-1 flex flex-col justify-center items-center text-center p-4 md:p-0">
          <img src="images/png/888/step3b@3x.png" class="w-16" alt="" />
          <div class="mt-8 mb-4 font-bold">Step 3</div>
          <div class="h-auto md:h-24">Fill in your details and answer a question</div>
        </div>
        
      </div>

    </div>







              <!-- participating brands -->
              <div class="flex flex-col justify-center items-center lg:mt-8 px-32 md:px-12 lg:px-36 2xl:px-36">
                <div class="h1 font-black text-xl sm:text-2xl flex my-4 items-center">Participating brands:</div>
                <img src="images/png/888/participatingbrands@3x.png" alt="" />
                
              </div>


            </div>
          </div>
        </div>
      </div>


    </div>


    





















    <div class="container mx-auto  relative overflow-hidden pb-48">
      <img src="images/png/888/bg-money@3x.png" class="absolute -bottom-12" style="width:100%;" alt="" />

      <div id="submitsection" class="overflow-hidden">
        <div class="w-full flex flex-col justify-center items-center  md:mx-auto lg:px-40 2xl:px-48 text-base">
          <div class="w-full relative  xl:px-30 2xl:px-20 ">
            <multi-form />

          </div>
        </div>
      </div>


    </div>

    <div id="tnc" class=" bg-white text-center absolute bottom-0 right-0 left-0">
      <div class="container mx-auto flex flex-col justify-center items-center py-6 md:px-10 2xl:px-40 text-xs md:text-sm">
      
        <!-- <img src="images/png/lotus/lotus-logo@3x.png" class="w-72 mb-4" alt="" /> -->
        <div class="  xl:px-10 2xl:px-20 ">© 2021 The Coca-Cola Company. ‘Coca-Cola’, ‘Sprite’, ‘Fanta’, ‘Minute Maid’, ‘Minute Maid Refresh’ and ‘Heaven and Earth’, ‘Authentic Tea House’ and Hut Device are trademarks of the Coca-Cola Company. 
         ‘A&amp;W is a trademark of European Refreshments.</div>
        <div><a class=" cursor-pointer underline" @click="openModal">Terms &amp; Conditions</a>&nbsp;|&nbsp; <a class=" cursor-pointer underline" @click="openModalPrivacy">Privacy Policy</a></div>

      </div>
    </div>














  </div>

</template>



<script>
  // Import the products query
  import VueTailwindModal from '~/components/VueTailwindModal.vue';
  // import CokeFormAlt from '~/components/CokeFormAlt.vue';
  import MultiForm from '~/components/MultiForm.vue';
  import CokePrivacy from '~/components/888/CokePrivacy.vue';
  import CokeTerms from '~/components/888/Coke888Terms.vue';

  export default {
    
  name :"main-page",
auth: false,
    components: {
      VueTailwindModal,
     //  CokeFormAlt,
      // MultiForm,
      CokePrivacy,
      CokeTerms
    },
    
    data() {
      return {
        
        isShowingStores: false,
        // Initialize an empty products variabkle
        products: [],
        query: '',
        fileName: '',
        isLoading: true,
        open: false,
        isUserScrolling: false,
        showing: false,
        showingPrivacy: false
      }
    },
    head() {
      return {
        title: "CNY 2022 CONVENIENCE & PETROL MART PROMOTION"
      }
    },
    created() {
      if (process.client){
        // this.$router.push(this.localePath({ name: 'countdown' }))
        
        // eslint-disable-next-line nuxt/no-globals-in-created
        window.addEventListener('scroll', this.handleScroll);

        console.log("this.$config.tattest",this.$config.tattest);
        console.log("this.$config.tattest2",this.$config.tattest2);
      }
    },
    mounted(){
      
    },
    destroyed() {
      window.removeEventListener('scroll', this.handleScroll);
    },
    methods: {

      openModal() {
        this.showing = true;
      },
      openModalPrivacy() {
        this.showingPrivacy = true;
      },
      signIn() {
      },
      close() {
        console.log("after close...")
      },
      handleScroll(event) {
        this.isUserScrolling = (window.scrollY > 0);
        // Any code to be executed when the window is scrolled
      }
    }
    
  }
</script>

