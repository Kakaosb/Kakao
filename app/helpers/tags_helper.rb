module TagsHelper

	def find_tags 
		

@article.text = @article.text+" "
	reg = @article.text.scan(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/)
	reg_1 =	reg
		
while reg != [] do
	
 
reg = reg [0] 
reg = reg.to_s[3...-2]
reg_2 =	reg
#добавление элемента в массив

#controller: 'tag' method: 'create'
@article.text = @article.text.sub(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/ , "<a href=\"#{reg}\"> ##{reg}</a>")
#@article.text = @article.text.sub(/#(?<![*>#])(#[А-Яа-я0-9a-zA-Z]*)/ , reg.upcase)
reg = @article.text.scan(/(#[А-Яа-я0-9a-zA-Z]*(?=[ |\s]))/)

#reg_3 =	reg

#@article.text = @article.text + "   reg_1:       " + reg_1.to_s+ "    reg_2:      " + reg_2.to_s + "    reg_3:      " +reg_3.to_s
end 

	end


end
