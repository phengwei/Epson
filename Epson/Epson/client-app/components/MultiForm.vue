
<template lang="pug">
.relative.w-full


  //
    img(src="images/png/bg.png" class="hidden sm:block" alt="")
    div(class=" z-20 sm:absolute sm:inset-0  flex flex-col justify-start items-center")
  .w-full.flex.flex-col.justify-start.items-center
  
    .w-full.h-36.bg-cover.bg-no-repeat.bg-body-frame-3.flex.flex-col.justify-start.items-center.-mb-2
      .h1.font-black.text-3xl.mt-12 Participate now 
    form#msform.w-full.z-20.bg-body-frame-middle-3.bg-contain.-mb-2(enctype="multipart/form-data")
      // progressbar
      ul#kc-progressbar.pl-0.pt-6
        li(v-for="(item,index) in data"  :class="{'active':index==0 }") 
          | {{item.title}}
      fieldset
        .flex.flex-col.justify-start(class='sm:mx-20') 
          .flex.flex-wrap.mb-6.w-full(class='sm:px-18 2xl:px-24 text-left ')
            .w-full.px-3(class='xl:pb-2 2xl:pb-4')
              label.block.tracking-wide.font-bold.mb-2(:class="{ ' text-red-700': errorlist.includes('fullname','fullname2') }" for='grid-fullname')
                | Full Name
              input#grid-fullname.appearance-none.block.w-full.border.border-white.bg-white.rounded-lg.py-3.px-4.mb-3.leading-tight(v-model='fullname' type='text' placeholder='' maxlength='100' class='focus:outline-none focus:border-red-600' @focus="popErrors(['fullname'])")
            .w-full.px-3(class='xl:pb-2 2xl:pb-4')
              label.block.tracking-wide.font-bold.mb-2(:class="{ ' text-red-700': errorlist.includes('email','email2') }" for='grid-email')
                | Email
              input#grid-email.appearance-none.block.w-full.border.border-white.bg-white.rounded-lg.py-3.px-4.mb-3.leading-tight(v-model='email' type='email' maxlength='100' class='focus:outline-none focus:border-red-600' @focus="popErrors(['email','email2'])")
            .w-full.px-3(class='xl:pb-2 2xl:pb-4')
              label.block.tracking-wide.font-bold.mb-2(for='grid-mobile' :class="{ ' text-red-700': errorlist.includes('mobile','mobile2') }")
                | Mobile
              input#grid-mobile.appearance-none.block.w-full.border.border-white.bg-white.rounded-lg.py-3.px-4.mb-3.leading-tight(v-model='mobile' type='text' maxlength='20' class='focus:outline-none focus:border-red-600' @focus="popErrors(['mobile','mobile2'])")
            .w-full.px-3(class='xl:pb-2 2xl:pb-4')
              label.block.tracking-wide.font-bold.mb-2(for='grid-postcode' :class="{ ' text-red-700': errorlist.includes('postcode','postcode2') }")
                | Postcode
              input#grid-postcode.appearance-none.block.w-full.border.border-white.bg-white.rounded-lg.py-3.px-4.mb-3.leading-tight(v-model='postcode' type='text' maxlength='5' class='focus:outline-none focus:border-red-600' @focus="popErrors(['postcode','postcode2'])" )
            .w-full.px-3.pb-4
              label.block.tracking-wide.font-bold.mb-2(for='grid-upload' :class="{ ' text-red-700': errorlist.includes('fileupload','fileupload2') }")
                | Upload Receipt
              .flex.flex-row.justify-end
                input#grid-fileobj.appearance-none.block.w-full.border.border-white.bg-white.rounded-l-lg.py-3.px-4.leading-tight(v-model='fileName' type='text' class='focus:outline-none focus:border-red-600')
                label.w-64.flex.flex-row.justify-around.items-center.border.border-red-700.py-3.rounded-r-lg.tracking-wide.cursor-pointer.text-white.bg-red-700.ease-linear.transition-all.duration-150
                  fa(:icon="['fas', 'upload']")
                  span.text-base.leading-normal Choose file
                  input#grid-upload.hidden(type='file' name='receipt' accept='application/pdf, image/*' @change='filesChange($event.target.name, $event.target.files); fileCount = $event.target.files.length' @focus="popErrors(['fileupload'])")
              p.mt-2.text-sm(class='sm:text-base')
                | JPG / PNG / PDF. Maximum file size 10MB. 
                br
                | Kindly ensure that the
                |           receipt attached is clear and ligible.
            .w-full.px-3.py-2.text-sm(class='2xl:py-8 sm:text-base')
              label.inline-flex.items-center
                input.text-red-500.w-8.h-8.mr-2.border.border-gray-300.rounded(v-model='tncchecked' class='focus:ring-red-400 focus:ring-opacity-25' type='checkbox')
                p
                  | I confirm that I have read, consent and agree to campaign 
                  a.text-gray-800.underline.cursor-pointer(@click='openModal') Terms &amp; Conditions
                  | , and I am above 18 years of age.
            .w-full.px-3.text-sm(class='sm:text-base')
              label.inline-flex.items-center
                input.text-red-500.w-8.h-8.mr-2.border.border-gray-300.rounded(v-model='tnc2checked' class='focus:ring-red-400 focus:ring-opacity-25' type='checkbox')
                p
                  | Check this box to subscribe to future promotions notification
            .w-full.px-3.py-3.text-center(class='md:py-6 xl:py-2 2xl:py-10')
              button#next1.kc-next.font-bold.bg-red-700.text-white.px-28.py-4.rounded-full( :class="{'text-gray-500':isLoading, 'text-white':!isLoading}" class='md:w-96 md:px-32' )
                | Next
                svg.inline-block.animate-spin.h-8.w-8(v-show='isLoading' xmlns='http://www.w3.org/2000/svg' fill='none' viewbox='0 0 24 24')
                  circle.opacity-25(cx='12' cy='12' r='10' stroke='currentColor' stroke-width='4')
                  path.opacity-75(fill='currentColor' d='M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z')



        
      fieldset
        .flex.flex-col.justify-start.w-full

          .flex.flex-wrap.mb-6.w-full(class='2xl:px-24 text-left lg:px-18')
            .w-full.px-3(class='xl:pb-2 2xl:pb-4') 
              .block(v-if="!covered")  
                .block(v-if="chosenQuestion") 
                  span.font-bold {{chosenQuestion.question}}
                  .mt-2
                    div(v-for="(option,optionindex) in chosenQuestion.options" :v-key="'qq-'+optionindex")
                      label.inline-flex.items-center
                        input.form-radio(type='radio' name='radio' :value='option'   v-model="pickedanswer")
                        span.ml-2 {{option}}
              .flex.flex-col(v-else)
                .h-48.flex.flex-col.justify-center.items-center
                  vue-countdown(v-if='counting' :time='5000' @end='onCountdownEnd' v-slot='{ totalSeconds }')
                    .h1.font-black.text-red-700.text-9xl {{ totalSeconds }} 
                  .flex.flex-col.text-xl.font-bold(v-else)
                    div.mb-4 
                      | Select the correct answer 
                      span.text-red-700 AS FAST AS YOU CAN 
                    div When you are ready, press start.
                    
                button( v-if="!counting" type='button' class=" bg-red-600 text-white  hover:border-red-200  ml-2 px-2 md:px-8  py-4  text-xl  rounded-full" :disabled="counting"   @click="startCountdown") START
     

           
                
        //
          input.action-button-previous(type='button'  @click="testanswer" value='Test Answer')
          input.action-button-previous(type='button'  @click="reshuffle" value='Restart')
          input.kc-previous.action-button-previous(type='button'  value='prev')

        button#next2.kc-next.action-button(v-show="!covered && !counting && pickedanswer !== '' " type='button' name='next' class="w-1/2 bg-red-600 text-white  hover:border-red-200  ml-2 px-2 md:px-8  py-4  text-xl  rounded-full")
          | next
          svg.inline-block.animate-spin.h-8.w-8.text-white(v-show='isLoading' xmlns='http://www.w3.org/2000/svg' fill='none' viewbox='0 0 24 24')
            circle.opacity-25(cx='12' cy='12' r='10' stroke='currentColor' stroke-width='4')
            path.opacity-75(fill='currentColor' d='M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z')




      fieldset
        .d-flex
          label.col-12.text-left.d-flex.font-black.text-xl( class="lg:text-xl") 
            | Thank you for your participation
            br
            br
            | You will be contacted via email if you are one of the winners.
        //
          input.kc-previous.action-button-previous(type='button' name='previous' value="BACK")

  .w-full.bg-contain.bg-no-repeat.bg-body-frame-bottom-3.flex.flex-col.justify-start.items-center( class="h-12 sm:h-36" )
        





</template>


<script>
    
// import createAonApp from '~/apollo/mutations/aon/createAonApp'
    import $ from 'jquery' 
const STATUS_INITIAL = 0; const STATUS_SAVING = 1; const STATUS_SUCCESS = 2; const STATUS_FAILED = 3; const STATUS_HASFILES = 4;

export default {


    
		data() {
			return {
              counterStartTime:null,
              counterEndTime:null,
              
              counting: false,
              covered: true,
              pickedanswer:"",
              randomindex: 0,
              qna:[
                {
                  question:"Chinese New Year 2022 is the year of the?",
                  options:["Rat","Ox","Tiger","Rabbit"],
                  answer:"Tiger"
                },
                {
                  question:"How many days in Chinese New Year celebrated",
                  options:["3 days","7 days","12 days","15 days"],
                  answer:"15 days"
                },
                
                {
                  question:"Which fruit represents wealth and fortune, and is traditionally popularly handed out for the Chinese New Year?",
                  options:["Durian","Mango","Mandarin Orange","Grapefruit"],
                  answer:"Mandarin Orange"
                },
                
                {
                  question:"What form of blessings do children usually receive from elders during Chinese New Year?",
                  options:["Firecrackers","Cookies","Peanuts","Hong Bao (red packets)"],
                  answer:"Hong Bao (red packets)"
                },
                
                {
                  question:"How many Zodiac signs are there in the Chinese astrology calendar?",
                  options:["6","8","10","12"],
                  answer:"12"
                },
                
                {
                  question:"Coca-cola's Chinese New Year 2022 Limited Edition cans has how many designs?",
                  options:["2","3","5","6"],
                  answer:"6"
                },
                
                {
                  question:"What is Chinese New Year 2022 campaign's tagline?",
                  options:["Coke Side of Life","There's Magic When We Eat Together","Open Happiness","Let's Celebrate Chinese New Year"],
                  answer:"There's Magic When We Eat Together"
                },
                
                {
                  question:"The following brands are sold and distributed by Coca-Cola Refreshments(M) Sdn. Bhd.",
                  options:["Authentic Tea House","Heaven & Earth","Minute Maid","All of the above"],
                  answer:"All of the above"
                },
                
                {
                  question:"Name the Coca-Cola variant which has zero sugar:",
                  options:["Coca-Cola Zero Sugar","Coca-Cola Stevia","Coca-Cola Vanilla","Coca-Cola Klasik"],
                  answer:"Coca-Cola Zero Sugar"
                },
                
                {
                  question:"The Coca-Cola iconic bottle is also called",
                  options:["Soda bottle","Contour Bottle","Glass Bottle","Coke Bottle"],
                  answer:"Contour Bottle"
                },
                
                {
                  question:"When was Coca-Cola first sold",
                  options:["1886","1888","1890","1896"],
                  answer:"1886"
                },
                
                {
                  question:"When was Coca-Cola first sold",
                  options:["1886","1888","1890","1896"],
                  answer:"1886"
                }
              ],
              timetaken: 0,

                tncchecked: false,
                tncchecked2: false,
                tnc2checked: false,
                createdKidzClub: null,
				      selectedRelationship: null,
                errors: [],
                
isLoading: false,
                fullname: null,
                nric: null,
                mobile: null,
                postcode: null,
                country: "Malaysia",
                email: null,
                fileCount: 0,
                fileName:'',
                
                errorlist: [],
                
                errordesc: {
                    'fullname':'Full Name required.',
                    'fullname2':'Full Name contains unallowed characters.',
                    'mobile':'Mobile number required.',
                    'mobile2':'Mobile number needs to start with country code (e.g. 6017*******)',
                    'postcode':'Postcode required.',
                    'postcode2': 'Malaysia postcode needs to be 5 digit.',
                    'nric':'IC No required.',
                    'email':'Email required.',
                    'email2':'Valid email required (e.g. yourname@domainname.com)',
                    'fileupload': 'Receipt file is required.',
                    'fileupload2': 'Receipt file is too large!',
                    'tnc': 'Please agree to Terms and Conditions',

                    "noanswer":"Please select an answer"
                    
                    
                },
                
                
                uploadedFiles: [],
                uploadError: null,
                currentStatus: null,
                uploadFieldName: 'photos',




                data:[
                            {
                                "title_my": "Maklumat Pemohon",
                                "prev": "",
                                "next": "Next",
                                "subtitle_my": "Beritahu kami maklumat dan cara perhubungan pemohon",
                                "subtitle_zh": "告诉我们关于申请人的资料和联络方法",
                                "title_zh": "申请人个人资料",
                                "subtitle": "Provide us the applicant's info",
                                "title": "Fill in your details",
                                "next_my": "Seterusnya",
                                "prev_my": "",
                                "fields": [
                                    {
                                        "name": "name",
                                        "title": "Full Name (As per MyKad)",
                                        "title_zh": "英语全名（根据MyKad）",
                                        "title_my": "Nama Penuh (MyKad)"
                                    },
                                    {
                                        "name": "nric",
                                        "title": "MyKad No",
                                        "title_zh": "MyKad号码",
                                        "title_my": "Nombor MyKad"
                                    },
                                    {
                                        "title": "Residential Address",
                                        "title_zh": "住址",
                                        "title_my": "Alamat Rumah"
                                    },
                                    {
                                        "title": "Contact Number",
                                        "title_zh": "联络电话号码",
                                        "title_my": "Nombor Telefon",
                                        "placeholder": "+6010-1234568"
                                    },
                                    {
                                        "title": "Whatsapp Number",
                                        "title_zh": "Whatsapp电话号码",
                                        "title_my": "Nombor Telefon Whatsapp",
                                        "subtitle": "if it is different from contact number",
                                        "subtitle_zh": "如果与联络电话号码不一样",
                                        "subtitle_my": "Kekiranya berlain drpd nombor telefon",
                                        "placeholder": "+6010-1234568"
                                    },
                                    {
                                        "title": "Email Address",
                                        "title_zh": "电邮地址",
                                        "title_my": "Alamat Emel"
                                    }
                                ],
                                "prev_zh": "",
                                "next_zh": "下一步"
                            },
                            {
                                "title_my": "Muat Naik Keputusan SPM",
                                "prev": "Back",
                                "next": "Submit your answer",
                                "subtitle_my": "Muat naik keputusan SPM atau peperiksaan lain yang sama taraf",
                                "subtitle_zh": "上载SPM成绩或者其他同等考试成绩",
                                "title_zh": "上载SPM成绩",
                                "subtitle": "Kindly upload SPM Result or equivalence",
                                "title": "Answer a Question",
                                "next_my": "Seterusnya",
                                "prev_my": "Sebelumnya",
                                "fields": [
                                    {
                                        "name": "upload",
                                        "title": "Please attach a copy of the SPM result of the applicant.",
                                        "title_zh": "请附上申请人的SPM成绩表",
                                        "title_my": "Sila menampah keputusan SPM pemohon",
                                        "subtitle": "Drag your file(s) here to begin \r\n  or click to browse",
                                        "subtitle_zh": "把您的文件拉进此框，\r\n 或点击以打开视窗浏览文件所在",
                                        "subtitle_my": "Drag fail anda ke sini \r\n atau klik untuk browse fail dari komputer atau telefon bimbit"
                                    }
                                ],
                                "prev_zh": "上一步",
                                "next_zh": "下一步"
                            },
                            {
                                "tnc_my": "Dengan memberikan maklumat yang dinyatakan dalam borang ini, saya memberikan keizinan kepada Tung Shin Hospital dan wakil mereka dan / atau ejen mereka mengumpulkan, menggunakan dan mendedahkan data peribadi saya untuk tujuan yang berkaitan. Tujuan tersebut dinyatakan dalam Dasar Privasi Data Peribadi, dapat diakses di https://www.tungshin.com.my/privacy-policy atau tersedia atas permintaan. \r\n\r\n Dengan ini saya mengesahkan bahawa semua data peribadi yang saya berikan semuanya benar, terkini dan tepat. Sekiranya terdapat perubahan pada data peribadi saya, saya akan segera memberitahu Tung Shin Hospital. \r\n\r\n Berkenaan dengan tujuan pemasaran dan promosi, saya mengizinkannya",
                                "tncagree": "I hereby confirm that all the personal data that I have provided is true and I agree with the Terms & Conditions and the PDPA Agreement. Before submitting this application, please read and accept the Terms & Conditions and the PDPA Agreement.",
                                "tncfooter": "*Only shortlisted candidates will be contacted.",
                                "tnc_zh": "通过提供此表格中列出的信息，我同意同善医院及其代表和/或代理收集、使用和披露我的个人数据，以为我提供任何合理相关的目的。这些目的清楚列明在《隐私政策》中，可从访问网页https://www.tungshin.com.my/privacy-policy 或应要求提供。 \r\n\r\n 我在此确认我提供的所有个人数据属实，最新和准确。如果我的任何个人数据有任何更改，我将立即通知同善医院。 \r\n\r\n关于营销和促销目的，我同意",
                                "pdpa": {
                                    "title": "Personal Data Protection Policy (PDPA)",
                                    "title_zh": "个人资料保护法案",
                                    "title_my": "Personal Data Protection Policy (PDPA)",
                                    "url": "/private-policy"
                                },
                                "tncfooter_my": "*Hanya calon yang disenarai pendek sahaja yang akan dihubungi.",
                                "tncagree_my": "Dengan ini saya mengesahkan bahawa semua data peribadi yang telah saya berikan adalah benar dan saya bersetuju dengan Terma & Syarat dan Perjanjian PDPA. Sebelum menghantar permohonan ini, sila baca dan terima Terma & Syarat dan Perjanjian PDPA.",
                                "tnc": "\r\n By providing the information set out in this form, I consent to Tung Shin Hospital and their representatives and/ or agents collecting, using and disclosing my personal data to provide me with any reasonably related purpose. Such purposes are set out in the Personal Data Privacy Policy, accessible at https://www.tungshin.com.my/Privacy-Policy or available on request. \r\n\r\n I hereby confirm that all personal data that I have provided are all true, up-to-date and accurate. Should there be any changes to any of my personal data, I shall notify Tung Shin Hospital immediately. \r\n\r\n With regards to marketing and promotional purposes, I consent that",
                                "tncfooter_zh": "*只有入围的候选人将被联系。",
                                "tncagree_zh": "我在此确认我提供的所有个人数据都是真实的，并且我同意条款和条件以及 PDPA 协议。在提交此申请之前，请阅读并接受条款和条件以及 PDPA 协议。",
                                "tncagree2": "I agree to receive any marketing messages or collaterals in/ from all mediums including and unlimited to any electronic, digital, physical form or through any applications.",
                                "title_my": "Hantar Permohonan",
                                "prev": "Previous",
                                "next": "Submit",
                                "subtitle_my": "Baca terma dan syarat dengan teliti sebelum hantar permohonan",
                                "subtitle_zh": "仔细阅读条款和条件，确认并提交申请。",
                                "title_zh": "递交申请",
                                "subtitle": "Read the terms and conditions carefully, acknowledge it and submit the application.",
                                "title": "Wait for Announcement!",
                                "next_my": "Hantar",
                                "prev_my": "Sebelumnya",
                                "prev_zh": "上一步",
                                "next_zh": "递交申请",
                                "tncagree2_my": "Saya bersetuju untuk menerima sebarang pesanan pemasaran atau jaminan di / dari semua medium termasuk dan tidak terhad kepada sebarang bentuk elektronik, digital, fizikal atau melalui sebarang aplikasi.",
                                "tncagree2_zh": "我同意以各种媒介接收任何营销信息或抵押品，包括但不限于任何电子，数字，物理形式或通过任何应用程序。"
                            }
                        ],
                pdpa:{
                    "title": "Personal Data Protection Policy (PDPA)",
                    "title_zh": "个人资料保护法案",
                    "title_my": "Personal Data Protection Policy (PDPA)",
                    "url": "/privacy-policy"
                }


                
			}
		},
    
		computed: {
      chosenQuestion (){
      
          const chosenQ = this.qna[this.randomindex];
          const list = chosenQ.options.sort(function(){return 0.5 - Math.random()});
          return {
            question : chosenQ.question,
            options : list,
            answer : chosenQ.answer
          };
          
      },
      
      isInitial() {
        return this.currentStatus === STATUS_INITIAL;
      },
      isSaving() {
        return this.currentStatus === STATUS_SAVING;
      },
      isSuccess() {
        return this.currentStatus === STATUS_SUCCESS;
      },
      isFailed() {
        return this.currentStatus === STATUS_FAILED;
      },
      hasFiles() {
        return this.currentStatus === STATUS_HASFILES;
      }
    },

      async mounted() {
        this.reshuffle();
          try {
            await this.$recaptcha.init()
          } catch (e) {
            console.error(e);
          }


        
        this.reset();
        
      const $vm = this;
        // jQuery time
        let currentfs, nextfs, previousfs; // fieldsets
        let left, opacity, scale; // fieldset properties which we will animate
        let animating; // flag to prevent quick multi-click glitches


        $(".kc-next#next1").click(function(e){
            
                console.log("e.target.id", e.target.id);
                // validate()
                const isValid = $vm.checkForm(e,e.target.id);
                if(!isValid){
                    const errors = [];
                    for(let i=0; i< $vm.errorlist.length; i++){
                        const err = $vm.errorlist[i];
                        const errdesc = $vm.errordesc[err];
                        if(errdesc != null){
                            errors.push(errdesc);
                        }
                    }
                    $vm.$swal('Form Incomplete!!', errors.join('<br/>'), 'error');
                    return false;
                }
                
                
                if(animating) return false;
                animating = true;
                
                currentfs = $(this).closest("fieldset");
                nextfs = currentfs.next();

                // activate next step on progressbar using the index of next_fs
                $("#kc-progressbar li").eq($("fieldset").index(nextfs)).addClass("active");

                // show the next fieldset
                nextfs.show(); 

                // hide the current fieldset with style
                currentfs.animate({opacity: 0}, {
                    step(now, mx) {
                        // as the opacity of current_fs reduces to 0 - stored in "now"
                        // 1. scale current_fs down to 80%
                        scale = 1 - (1 - now) * 0.2;
                        // 2. bring next_fs from the right(50%)
                        left = (now * 50)+"%";
                        // 3. increase opacity of next_fs to 1 as it moves in
                        opacity = 1 - now;
                        currentfs.css({
                          'transform': 'scale('+scale+')'
                        });
                        nextfs.css({left, opacity});
                    }, 
                    duration: 800, 
                    complete(){
                        currentfs.hide();
                        animating = false;
                    }, 
                    // this comes from the custom easing plugin
                    easing: 'easeInOutBack'
                });

                e.preventDefault();
                
        });




        $(".kc-next#next2").click(function(e){
            
                // validate()
                const isValid = $vm.checkForm(e,e.target.id);
                if(!isValid){
                    const errors = [];
                    for(let i=0; i< $vm.errorlist.length; i++){
                        const err = $vm.errorlist[i];
                        const errdesc = $vm.errordesc[err];
                        if(errdesc != null){
                            errors.push(errdesc);
                        }
                    }
                    $vm.$swal('Form Incomplete!!', errors.join('<br/>'), 'error');
                    return false;
                }
                
                
                if(animating) return false;
                animating = true;
                


              if(e.target.id === "next2"){
                  // console.log("in next2");
                  $vm.timetaken = Date.now() - $vm.counterStartTime;

                  const res = $vm.onSubmit();
                  if(res instanceof Error){
                      $vm.$swal('Ops! Sorry', 'Unable to submit, error:'+res, 'error')
                      .then((result) => {
                              this.isLoading = false;
                      });

                  }else{
                      res.then((res) => {
                          $vm.answer();
                          $vm.isLoading = false;
                          console.log('res', res)


                          currentfs = $(this).closest("fieldset");
                          nextfs = currentfs.next();

                          // activate next step on progressbar using the index of next_fs
                          $("#kc-progressbar li").eq($("fieldset").index(nextfs)).addClass("active");

                          // show the next fieldset
                          nextfs.show(); 

                          // hide the current fieldset with style
                          currentfs.animate({opacity: 0}, {
                              step(now, mx) {
                                  // as the opacity of current_fs reduces to 0 - stored in "now"
                                  // 1. scale current_fs down to 80%
                                  scale = 1 - (1 - now) * 0.2;
                                  // 2. bring next_fs from the right(50%)
                                  left = (now * 50)+"%";
                                  // 3. increase opacity of next_fs to 1 as it moves in
                                  opacity = 1 - now;
                                  currentfs.css({
                                    'transform': 'scale('+scale+')'
                                  });
                                  nextfs.css({left, opacity});
                              }, 
                              duration: 800, 
                              complete(){
                                  currentfs.hide();
                                  animating = false;
                              }, 
                              // this comes from the custom easing plugin
                              easing: 'easeInOutBack'
                          });

                          e.preventDefault();
                      })
                  }
                 
                 
                   

              }
 

                
        });

        $(".kc-previous").click(function(e){
            if(animating) return false;
            animating = true;

            currentfs = $(this).parent();
            previousfs = $(this).parent().prev();

            // de-activate current step on progressbar
            $("#kc-progressbar li").eq($("fieldset").index(currentfs)).removeClass("active");

            // show the previous fieldset
            previousfs.show(); 
            // hide the current fieldset with style
            currentfs.animate({opacity: 0}, {
                step(now, mx) {
                    // as the opacity of current_fs reduces to 0 - stored in "now"
                    // 1. scale previous_fs from 80% to 100%
                    scale = 0.8 + (1 - now) * 0.2;
                    // 2. take current_fs to the right(50%) - from 0%
                    left = ((1-now) * 50)+"%";
                    // 3. increase opacity of previous_fs to 1 as it moves in
                    opacity = 1 - now;
                    currentfs.css({left});
                    previousfs.css({'transform': 'scale('+scale+')', opacity});
                }, 
                duration: 800, 
                complete(){
                    currentfs.hide();
                    animating = false;
                }, 
                // this comes from the custom easing plugin
                easing: 'easeInOutBack'
            });
            
            e.preventDefault();
        });

        $(".kc-submit").click(function(){
            return false;
        })






    },



    beforeDestroy() {
          this.$recaptcha.destroy()
    },
    methods: {  
    refreshToken(){
      console.log("refreshToken");
      this.$auth.refreshTokens();
    },
      
    startCountdown() {
      this.counting = true;
    },
    onCountdownEnd() {
      this.counting = false;
      this.covered = false;
      this.counterStartTime = Date.now();

    },
    reshuffle(){
      this.covered = true;
      this.randomindex = this.randomNumber();
    },
    answer(){

      this.reshuffle();
      this.covered = true;

         /**
         this.$swal(
                  'Thank you',
                  'We have received your submission',
                  'success'
                ).then((result) => {
                   this.reshuffle();
                   this.covered = true;
                  // window.location.reload();
                })

                */

      },
      testanswer(){
          const isCorrect = this.qna[this.randomindex].answer === this.pickedanswer;

        if(isCorrect){
          this.$swal(
                  'Congratulation!',
                  'Your have chosen the correct answer!',
                  'success'
                ).then((result) => {
                  // this.reshuffle();
                  // this.covered = true;
                  // window.location.reload();
                })
        }else{
            this.$swal(
                  'Sorry!',
                  'Your have chosen the incorrect answer!',
                  'error'
                ).then((result) => {
                  // window.location.reload();
                })

        }
          

      },
      randomNumber() {
        return  Math.floor(Math.random() * (10 - 1 + 1)) + 1
      },
      randomList(rand){
        return rand.sort(function(){return 0.5 - Math.random()});
      },
 openModal(){
      this.$parent.openModal();
 },





    async onSubmit(e) {

      this.isLoading = true;
      const isValid = this.checkForm(e)

      if (!isValid) {
        const errors = []
        for (let i = 0; i < this.errorlist.length; i++) {
          const err = this.errorlist[i]
          const errdesc = this.errordesc[err]
          if (errdesc != null) {
            errors.push(errdesc)
          }
        }
          this.$swal('Unable to proceed!', errors.join('<br/>'), 'error')
          .then((result) => {
                this.isLoading = false;
            });

        return false;
      }
      const formaction = "submitcontest";
      let payload = {
        fullname: this.fullname,
        postcode: this.postcode,
        mobile: this.mobile,
        email: this.email,
        action: formaction,
      }


      // Wait until recaptcha has been loaded.
      // await this.$recaptchaLoaded();


      const token = await this.$recaptcha.execute('submitcontest')
      payload.token = token
      try {
        // const source = this.$axios.CancelToken.source();
        const endpoint = `${this.$config.restUrl}/api/contests`

        payload = JSON.stringify(payload)
        const formData = new FormData()


        for (let ii = 0; ii < this.uploadedFiles.length; ii++) {
          const item = this.uploadedFiles[ii]
          formData.append('receipt', item.file, item.name)
        }

        formData.append('fullname', this.fullname);
        formData.append('postcode', this.postcode);
        formData.append('country', this.country);
        formData.append('mobile', this.mobile);
        formData.append('email', this.email);
        formData.append('action', formaction);

        
        
        const isCorrect = this.qna[this.randomindex].answer === this.pickedanswer;
        

        formData.append('answer', this.pickedanswer);
        formData.append('question', this.qna[this.randomindex].question);
        formData.append('timetaken', this.timetaken);
        formData.append('correct', isCorrect);


        formData.append('token', token);
        formData.append("body", JSON.stringify(payload));



        return this.$axios
          .post(endpoint, formData, {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          })
          .then((res) => {
            return res;
            /**
                console.log('res', res)
                if (res && res.data) {
                      if (res.data.code === 200) {
                        this.$swal(
                          'Congratulation!',
                          'Your application has been submitted successfully.',
                          'success'
                        ).then((result) => {
                          window.location.reload();
                        })

                        
                      } else {

                                  
                            this.$swal('Ops! Sorry', 'Unable to submit', 'error')
                            .then((result) => {
                                    this.isLoading = false;
                                });

                      
                      }
                }**/
          })
          
      } catch (error) {
          return error;
          /**
          this.$swal('Ops! Sorry', 'Unable to submit, error:'+error, 'error')
        .then((result) => {
                this.isLoading = false;
            });
        **/
      }
    },



    async onSubmitTest(e) {

      this.isLoading = true;


      // Wait until recaptcha has been loaded.
      // await this.$recaptchaLoaded();


      const token = await this.$recaptcha.execute('submitcontest')
      try {
        // const source = this.$axios.CancelToken.source();
        const endpoint = `${this.$config.restUrl}/api/contests`

        const formData = new FormData()

        for (let ii = 0; ii < this.uploadedFiles.length; ii++) {
          const item = this.uploadedFiles[ii]
          formData.append('receipt', item.file, item.name)
        }

        formData.append('fullname', "fullname");
        formData.append('postcode', "postcode");
        formData.append('country', "country");
        formData.append('mobile', "mobile");
        formData.append('email', "email");
        formData.append('action', "formaction");

        

        formData.append('answer', "pickedanswer");
        formData.append('question', "question");
        formData.append('timetaken', 1000);
        formData.append('correct', true);
        

        formData.append('token', token);



        return this.$axios
          .post(endpoint, formData, {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          })
          .then((res) => {
            return res;
           
          })
          
      } catch (error) {
        console.log("error",error);
          this.$swal('Error!', error, 'error');
          e.preventDefault();
         
      }
    },



    filesChange(fieldName, fileList) {
      this.uploadedFiles = [];
      // handle file changes
      if (!fileList.length) return
      Array.from(Array(fileList.length).keys())
        // eslint-disable-next-line array-callback-return
        .map((x) => {
          this.uploadedFiles.push({
            // eslint-disable-next-line object-shorthand
            fieldName: fieldName,
            file: fileList[x],
            name: fileList[x].name,
          })

          this.fileName = fileList[x].name;
          
        })
         

            if (this.uploadedFiles != null && this.uploadedFiles[0] != null) {
              const fileExt = (/[.]/.exec(this.uploadedFiles[0].name)) ? /[^.]+$/.exec(this.uploadedFiles[0].name) : undefined;
              console.log("fileExt",fileExt[0]);


              // if(fileExt[0] )
              const regex = /[^\\s]+(.jpg|.jpeg|.gif|.png|.bmp|.pdf)$/
              const extOK = regex.test(this.uploadedFiles[0].name);

              console.log("extOK", extOK);
              if(extOK === false){
                    this.$swal('Invalid file', "Only accepting JPG / PNG / PDF. Maximum file size 10MB", 'error')
                    .then((result) => {
                        this.uploadedFiles = [];
                        this.fileName = "";
                        this.fileCount = 0;
                    })
              }
              // regex = “([^\\s]+(\\.(?i)(jpe?g|png|gif|bmp))$)”; 
            }

            this.popErrors(['fileupload']);
            this.popErrors(['fileupload2']);
    },


    reset() {
      // reset form to initial state
      this.currentStatus = STATUS_INITIAL;
      this.uploadedFiles = [];
      this.uploadError = null;
    },


      
         changeTnc (event) {
        },
         popErrors(keys){
             for(let i=0; i< keys.length; i++){
                const key = keys[i];
                const index = this.errorlist.indexOf(key);
                if (index >= 0) {
                  this.errorlist.splice( index, 1 );
                }
             }
            
         },
         pushError(key){
            const index = this.errorlist.indexOf(key);
            if (index < 0) {
              this.errorlist.push(key);
            }
         },
          checkForm (e, formid) {
              this.errors = [];

               if(formid === "next1"){


                      if (!this.fullname || this.fullname.trim() === "") {
                            this.pushError("fullname");
                      }else{
                            this.popErrors(["fullname"]);
                            
                            const sanitizedName = this.$sanitize(this.fullname);  
                            if(sanitizedName !== this.fullname){
                                  this.pushError("fullname2");
                            }else{
                                  this.popErrors(["fullname2"]);
                            }
                      }


                      if (!this.mobile  || this.mobile.trim() === "") {
                            this.pushError("mobile");

                      }else{
                            this.popErrors(["mobile"]);

                            if(!this.validPhone(this.mobile)){
                                  this.pushError("mobile2");
                            }else{
                                  this.popErrors(["mobile2"]);
                            }
                      }

                     

                      
                      if (!this.postcode  || this.postcode.trim() === "") {
                            this.pushError("postcode");
                      }else{
                            this.popErrors(["postcode"]);


                            if(!this.validPostcode(this.postcode)){
                                  this.pushError("postcode2");
                            }else{
                                  this.validPostcode(["postcode2"]);
                            }
                            
                      }


                      if (!this.email || this.email.trim() === "") {
                          this.pushError("email");
                      } else{
                          this.popErrors(["email"]);
                           if(!this.validEmail(this.email)){
                                  this.pushError("email2");
                            }else{
                                  this.popErrors(["email2"]);
                            }
                      }

                      
                      if (this.fileCount <= 0) {
                        this.pushError('fileupload')
                      } else {
                        this.popErrors(['fileupload'])
                        // console.log("file size", (this.uploadedFiles[0].file.size / 1024 / 1024) + "");
                        
                        if((this.uploadedFiles[0].file.size / 1024 / 1024) > 10){
                          this.pushError('fileupload2');
                        }else{
                          this.popErrors(["fileupload2"]);
                        }
                      }

                      if (!this.tncchecked) {
                        this.pushError('tnc')
                      } else {
                        this.popErrors(['tnc'])
                      }

                
                      if (!this.errorlist.length) {
                        return true
                      }

                        

                      e.preventDefault()

                    
               }else if(formid === "next2"){

                      if (this.pickedanswer ==="") {
                        this.pushError('noanswer')
                      } else {
                        this.popErrors(['noanswer'])
                      }
                  // this.counterEndTime = Date.now();
                  // this.timetaken = Date.now() - this.counterStartTime;

                  // await this.onSubmit();
        
                  // this.answer();
                   
                   
               }
             
               
              if (!this.errorlist.length) {
                return true;
              }
               
               
               

              e.preventDefault();
            },

          
          validEmail (email) {
            const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(email);
          },
          
          validPhone (phonenum) {
            const re = /^(\+?6?01)[0-46-9]-*[0-9]{7,8}$/;
            return re.test(phonenum);
          },

          
          validPostcode (postcode) {
            const re = /^\d{5}$/;
            return re.test(postcode);
          }

          	
        
         
              
      }
}
</script>
