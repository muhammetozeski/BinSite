namespace BinSite.Social.Services;

/// <summary>Original editorial prompts; selecting one fills a draft and never fabricates a public user post.</summary>
public static class EditorialPrompts
{
    public static readonly StoryPrompt[] Stories =
    [
        new("Son vapurdaki mektup", "Vapurun son yolcusu inince koltuğun üzerinde bir mektup kaldı. Zarfın üstünde kaptanın adı, içinde ise yarın sabah geçeceği bütün duraklar yazılıydı. Listenin en altında denizde bulunmayan bir ada vardı."),
        new("Unutulan pazartesi", "Kasabadaki herkes salı sabahı uyandı. Telefonlar, takvimler ve gazeteler pazartesinin hiç yaşanmadığını söylüyordu. Yalnızca fırıncı, pazartesi pişirdiği ve henüz sıcak olan ekmeği elinde tutuyordu."),
        new("Komşunun geleceği", "Her gece saat üçte yan daireden henüz yazılmamış bir şarkı duyuluyordu. Melodi bir gün radyoda çaldığında, sunucu bestecinin dün doğan bir bebek olduğunu söyledi. Kapıyı çalmak için elini kaldırdı."),
        new("Gökyüzü kütüphanesi", "Şehirde ilk kez yağmur yerine kitap ayraçları yağdı. Her ayracın üzerinde farklı bir insanın en son okuduğu cümle vardı. Çocuk, kendi ayracını bulduğunda cümlenin henüz başlamadığını fark etti."),
        new("İkinci el gölgeler", "Dükkân yalnızca güneş batarken açılıyordu. Vitrinde sahiplerinden ayrılmış gölgeler, küçük etiketlerle bekliyordu. En pahalı gölge hiç kimseye benzemiyordu; dükkân sahibi onun yarına ait olduğunu söyledi."),
        new("Sessiz istasyon", "Tren istasyonundaki bütün anonslar bir hafta önce sustu. Sonra bir sabah yalnızca tek bir perondan fısıltı geldi: 'Bu trene binersen kaybettiğin bir şeyi bulacaksın, ama onun ne olduğunu seçemeyeceksin.'"),
        new("Kırmızı düğmeli terzi", "Terzi her cekete görünmeyen bir cep dikiyordu. Müşteriler ceplerine ellerini soktuklarında bir eşyaya değil, unuttukları bir anıya dokunuyordu. Bir gün dükkâna bütün cepleri boşalmış bir kadın geldi."),
        new("Denizin gönderdiği fatura", "Balıkçı ağını çektiğinde içinde mühürlü bir fatura buldu. Denizin aldığı bütün eşyalar tek tek yazılmıştı. En altta, kimsenin hatırlamadığı bir geminin iade işlemi için bir imza isteniyordu."),
        new("Son yetişkin", "Mahallenin çocukları bir sabah bütün yetişkinlerin oyun oynamayı unuttuğunu gördü. Parktaki yaşlı bekçi kuralları hatırlayan tek kişiydi. Çocukları yanına çağırdı ve yere hiç görmedikleri bir seksek çizdi."),
        new("Bir oda fazla", "Ev yıllardır aynı evdi: iki oda, bir salon. Ama elektrikler kesilince koridorun sonunda üçüncü bir kapı belirdi. Kapının altından gelen ışıkta, evin otuz yıl sonraki hâli görünüyordu."),
        new("Kayıp sesler tamircisi", "Tamircinin raflarında kavanozlar dolusu ses vardı: kapanmayan bir kapının gıcırtısı, çoktan yıkılmış bir sinemanın alkışı, söylenmemiş bir özür. Çırak, kendi adının yazıldığı kavanozu açtı."),
        new("Ayın iade günü", "Ay bir sabah her zamankinden daha yakın göründü. Belediyeye gelen tek sayfalık dilekçede, gökyüzüne asılan bütün dileklerin sahiplerine iade edileceği yazıyordu. İlk paket apartmanın çatısına düştü."),
        new("Dakika bahçesi", "Bahçıvan çiçek yerine dakikalar yetiştiriyordu. Bazıları kısa bir kahkaha kadar hızlı, bazıları vedalar kadar ağırdı. Şehrin bütün saatleri durduğunda bahçede açmamış tek bir tomurcuk kaldı."),
        new("Görünmez orkestranın şefi", "Konser salonunda hiçbir müzisyen yoktu ama bütün koltuklar doluydu. Şef elini kaldırınca herkes kendi çocukluğundan gelen bir enstrüman duydu. Ön sıradaki kadın ise yarından gelen bir ses işitti."),
        new("Dönüş biletinin arkası", "Biletin ön yüzünde evin adresi, arkasında ise henüz tanışmadığı birinin el yazısı vardı. 'Döndüğünde kapıyı üç kez çalma' diyordu. Otobüs şoförü bileti görünce ilk kez yolculardan birine yerini verdi.")
    ];

    public static readonly PollPrompt[] Polls =
    [
        new("Mahalleye tek bir yer açılacak: hangisi?", "Gece açık masa oyunu kütüphanesi", "Kendi kısa filmini çekebildiğin mini stüdyo"),
        new("Bir şehir festivalinin ana etkinliği ne olsun?", "Sokak sokak ilerleyen ortak hikâye oyunu", "Herkesin bir sahne tasarladığı sessiz konser"),
        new("Yeni bir sosyal ağda en çok neyi istersin?", "Birlikte bir şey üreten küçük gruplar", "Birbirine meydan okuyan günlük yaratıcı görevler"),
        new("Arkadaş grubuyla hafta sonu hangi oyunu oynarsın?", "Şehirde ipucu toplayarak çözülen gizem", "Evde nesneleri konuşturan doğaçlama yarışması"),
        new("Bir oyun yalnızca tek kurala sahip olsa hangisi?", "Her hamlede bir önceki kuralı değiştir", "Rakibine yardım etmeden puan alamazsın"),
        new("Bir topluluk uygulamasının kalbi ne olsun?", "Herkesin bir saat öğretebildiği yetenek takası", "Tamamen yabancıların tamamladığı ortak projeler"),
        new("Bir müzede en çok neyi deneyimlemek istersin?", "Bitmemiş icatları ziyaretçilerin tamamlaması", "Bir nesnenin geçmişini seslerle keşfetmek"),
        new("İnternetteki bir eğlence kanalında hangisine dönersin?", "İzleyicinin kuralları değiştirdiği yarışmalar", "Her bölümün sonunu farklı bir kişinin yazdığı dizi"),
        new("Yeni bir kelime oyununun farkı ne olsun?", "Kelimeyi tarif etmek yerine sesini çizmek", "Harfleri arkadaşının bulmacasında kazanmak"),
        new("Bir kutlama için hangi sürprizi hazırlarsın?", "Her arkadaşın bir ipucu bıraktığı dijital hazine", "Bir yıl sonra açılacak ortak zaman kapsülü"),
        new("Bir ortak çalışma alanında hangi fikir daha çekici?", "Gürültü yerine konuya göre değişen ses odaları", "Birbirinin projesine on dakika ayıran ekip eşleşmesi"),
        new("Bir hikâye uygulaması okuru nasıl oyuna alsın?", "Her paragrafta yepyeni bir yazar devralsın", "Okurlar karakterin neyi hatırladığını seçsin"),
        new("Bir kafe menüsünde eğlenceli olan hangisi?", "Tarifini müşterilerin oyladığı haftalık içecek", "Bir içeceğe bağlı küçük bir masaüstü gizemi"),
        new("Bir yarışmada hangi başarı daha anlamlı olsun?", "En az hamleyle herkese kazandırmak", "Kendi çözümünü bir başkasına öğretebilmek"),
        new("Dijital bir koleksiyonun amacı ne olsun?", "Ziyaret ettiğin yerlerin seslerinden kendi haritan", "Öğrendiğin şeylerin birbirine bağlandığı kişisel atlas")
    ];
}
